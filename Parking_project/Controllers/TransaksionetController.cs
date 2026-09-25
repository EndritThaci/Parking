using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking_project.Data;
using Parking_project.Helper;
using Parking_project.Models;
using Parking_project.Models.DTO;
using Stripe;
using System.Security.Claims;

namespace Parking_project.Controllers
{
    [ApiController]
    [Route("api/transaksionetParkimit")]
    public class TransaksionetController : Controller
    {
        private readonly AplicationDbContext _db;
        private readonly IMapper _mapper;
        public TransaksionetController(AplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet("{transaksioniId:int}")]
        [ProducesResponseType(typeof(ApiResponse<TransaksionRead>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<TransaksionRead>>> getTransaksionin(int transaksioniId)
        {
            try
            {
                var gettransaksioni = await _db.TransaksionParkimi.Where(t=> t.TransaksioniId==transaksioniId).Include(c=> c.Cilsimet).Include(c => c.User).FirstOrDefaultAsync();
                if (gettransaksioni == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Transaksioni with id {transaksioniId} not found."));
                }

                var getNjesia = await _db.NjesiOrg.Where(v => v.NjesiteId == gettransaksioni.NjesiaId).FirstOrDefaultAsync();
                if (getNjesia == null)
                {
                    return NotFound(ApiResponse<object>.NotFound("Njesia not found"));
                }

                var getSherbimet = await _db.TransaksionDetaj.Where(t => t.TransaksionId == transaksioniId).Include(c => c.Sherbimi).ToListAsync();

                List<Sherbimi> getSherbim = getSherbimet.Select(c => c.Sherbimi).ToList();

                TransaksionRead transaksionRead = new TransaksionRead
                {
                    TransaksioniId = gettransaksioni.TransaksioniId,
                    KohaDaljes = gettransaksioni.KohaDaljes,
                    KohaHyrjes = gettransaksioni.KohaHyrjes,
                    Cmimi = getSherbimet.Where(i => i.TransaksionId == transaksioniId).Sum(c=> c.Cmimi),
                    Statusi = gettransaksioni.Statusi,
                    Njesia = getNjesia,
                    Cilsimi = gettransaksioni.Cilsimet,
                    Useri = gettransaksioni.User,
                    Sherbimi = getSherbim.ToList()
                };

                return Ok(ApiResponse<TransaksionRead>.Ok(transaksionRead, "Transaksion retrived successfully"));

            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing the request.", innerMessage));

            }

        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<TransaksionRead>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<TransaksionPage>>> GetTransaksionin(int pageNumber = 1, int pageSize = 10, int njesiaId = -1)
        {
            try
            {
                if (pageNumber <= 0 || pageSize <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Page number and page size must be greater than zero."));
                }

                int orgId = int.Parse(User.FindFirst("BiznesId")!.Value);

                var query = _db.TransaksionParkimi
                    .AsNoTracking()
                    .Include(t => t.Cilsimet)
                        .ThenInclude(c => c.Sherbimi)
                    .Include(t => t.Cilsimet)
                        .ThenInclude(c => c.NjesiOrg)
                    .Include(t => t.Njesia)
                    .Include(t => t.User)
                    .Where(t => t.Cilsimet.Sherbimi.BiznesId == orgId);

                var transaksionQuery = _db.TransaksionDetaj.Where(d => d.TransaksionParkimi.Cilsimet.Sherbimi.BiznesId == orgId && d.TransaksionParkimi.Statusi != "Pending");
                if (njesiaId >= 0)
                {
                    njesiaId = njesiaId == 0 ? int.Parse(User.FindFirst("NjesiaId")!.Value) : njesiaId;
                    query = query.Where(t => t.Cilsimet.NjesiteId == njesiaId);
                    transaksionQuery = transaksionQuery.Where(t=> t.TransaksionParkimi.Cilsimet.NjesiteId == njesiaId);
                }

                var njesite = await _db.NjesiOrg.Where(o=> o.BiznesId == orgId).ToListAsync();

                var totalRecords = await query.CountAsync();

                if (totalRecords == 0)
                {
                    var rez = new TransaksionPage
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalPages = 0,
                        TotalRecords = totalRecords,
                        TotalAmount = 0,
                        MonthlyAmount = 0,
                        YearlyAmount = 0,
                        Njesite = njesite,
                        Data = new List<TransaksionRead>()
                    };
                    return Ok(ApiResponse<TransaksionPage>.Ok(rez, "No transactions found."));
                }

                var totalPages = (int)Math.Ceiling(totalRecords/(double)pageSize!);
                var totalAmount = await transaksionQuery.SumAsync(d => d.Cmimi);

                var now = DateTime.UtcNow;
                var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var monthEnd = monthStart.AddMonths(1);
                var monthlyAmount = await transaksionQuery
                    .Where(d => d.TransaksionParkimi.KohaHyrjes >= monthStart && d.TransaksionParkimi.KohaHyrjes < monthEnd)
                    .SumAsync(d => d.Cmimi);

                var yearStart = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var yearEnd = yearStart.AddYears(1);
                var yearlyAmount = await transaksionQuery
                    .Where(d => d.TransaksionParkimi.KohaHyrjes >= yearStart && d.TransaksionParkimi.KohaHyrjes < yearEnd)
                    .SumAsync(d => d.Cmimi);

                var transaksionet = await query
                    .OrderByDescending(t => t.TransaksioniId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var transaksionIds = transaksionet
                    .Select(t => t.TransaksioniId)
                    .ToList();

                var getSherbimet = await _db.TransaksionDetaj
                    .AsNoTracking()
                    .Where(c => transaksionIds.Contains(c.TransaksionId))
                    .Include(c => c.Sherbimi)
                    .ToListAsync();

                var result = transaksionet.Select(t => new TransaksionRead
                {
                    TransaksioniId = t.TransaksioniId,
                    KohaHyrjes = t.KohaHyrjes,
                    KohaDaljes = t.KohaDaljes,
                    Cmimi = getSherbimet.Where(i => i.TransaksionId == t.TransaksioniId).Sum(c => c.Cmimi),
                    Statusi = t.Statusi,
                    Identifikues = t.Identifikues,
                    Njesia = t.Njesia,
                    Cilsimi = t.Cilsimet,
                    Useri = new Useri
                        {
                            UserId = t.User.UserId,
                            Emri = t.User.Emri,
                            Mbiemri = t.User.Mbiemri,
                            Email = t.User.Email,
                            Role = t.User.Role,
                            Passwordi = "",
                            active = t.User.active
                        },
                    Sherbimi = getSherbimet.Where(d => d.TransaksionId == t.TransaksioniId).Select(d => d.Sherbimi).ToList()
                }).ToList();

                var page = new TransaksionPage
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords,
                    TotalAmount = totalAmount,
                    MonthlyAmount = monthlyAmount,
                    YearlyAmount = yearlyAmount,
                    Njesite = njesite,
                    Data = result
                };

                return Ok(ApiResponse<TransaksionPage>.Ok(page, "Transactions retrieved successfully"));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing the request.", innerMessage));
            }
        }

        [HttpGet]
        [Authorize]
        [Route("ByNjesi")]
        [ProducesResponseType(typeof(ApiResponse<TransaksionPage>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<TransaksionPage>>> GetTransaksioninByNjesi(int? njesia, string? search, DateTime? dateFrom, DateTime? dateTo, string? status, int page = 1, int pageSize = 10)
        {
            try
            {
                if (page <= 0 || pageSize <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Page number and page size must be greater than zero."));
                }

                int njesiaId = njesia ?? 0;
                if (User.IsInRole("Manager") || User.IsInRole("Employee"))
                {
                    njesiaId = int.Parse(User.FindFirst("NjesiaId")!.Value);
                }
                else if (njesiaId == 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Njesia is required for your role and it can't be Id = 0"));
                }

                var query = _db.TransaksionParkimi
                    .Include(t => t.Cilsimet)
                        .ThenInclude(c => c.Sherbimi)
                    .Include(n=> n.Njesia)
                    .Include(t => t.User)
                    .Where(t => t.Cilsimet.NjesiteId == njesiaId);

                if (dateFrom != null) query = query.Where(t => t.KohaHyrjes >= TimeZoneConverter.KosovoTimeToUtc(dateFrom.Value));
                if (dateTo != null) query = query.Where(t => t.KohaHyrjes <= TimeZoneConverter.KosovoTimeToUtc(dateTo.Value));
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(t => t.Statusi.ToLower().Contains(status.ToLower().Trim()));
                }
                if (!string.IsNullOrEmpty(search))
                {
                    var searchVal = search.ToLower().Trim();
                    query = query.Where(t => 
                        (t.Identifikues != null && t.Identifikues.ToLower().Contains(searchVal)) || 
                        t.User.Emri.ToLower().Contains(searchVal) || 
                        t.User.Mbiemri.ToLower().Contains(searchVal) ||
                        t.User.Email.ToLower().Contains(searchVal));
                }

                var totalRecords = await query.CountAsync();
                if (totalRecords == 0)
                {
                    var rez = new TransaksionPage
                    {
                        PageNumber = page,
                        PageSize = pageSize,
                        TotalPages = 0,
                        TotalRecords = totalRecords,
                        TotalAmount = 0,
                        MonthlyAmount = 0,
                        YearlyAmount = 0,
                        Njesite = new List<NjesiOrg>(),
                        Data = new List<TransaksionRead>()
                    };
                    return Ok(ApiResponse<TransaksionPage>.Ok(rez, "No transactions found."));
                }
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize!);

                var nowKs = TimeZoneConverter.UtcToKosovoTime(DateTime.UtcNow);

                var totalTransactionsToday = await _db.TransaksionParkimi
                    .CountAsync(t =>
                        t.Cilsimet.NjesiteId == njesiaId &&
                        t.KohaHyrjes >= TimeZoneConverter.KosovoTimeToUtc(nowKs.Date) &&
                        t.KohaHyrjes < TimeZoneConverter.KosovoTimeToUtc(nowKs));

                var transaksionet = await query
                    .OrderByDescending(t => t.TransaksioniId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var transaksionIds = transaksionet
                    .Select(t => t.TransaksioniId)
                    .ToList();

                var getSherbimet = await _db.TransaksionDetaj
                    .AsNoTracking()
                    .Where(c => transaksionIds.Contains(c.TransaksionId))
                    .Include(c => c.Sherbimi)
                    .ToListAsync();

                var result = transaksionet.Select(t => new TransaksionRead
                {
                    TransaksioniId = t.TransaksioniId,
                    KohaHyrjes = t.KohaHyrjes,
                    KohaDaljes = t.KohaDaljes,
                    Cmimi = getSherbimet.Where(i => i.TransaksionId == t.TransaksioniId).Sum(c => c.Cmimi),
                    Statusi = t.Statusi,
                    Identifikues = t.Identifikues,
                    Njesia = t.Njesia,
                    Cilsimi = t.Cilsimet,
                    Useri = new Useri
                        {
                            UserId = t.User.UserId,
                            Emri = t.User.Emri,
                            Mbiemri = t.User.Mbiemri,
                            Email = t.User.Email,
                            Role = t.User.Role,
                            Passwordi = "",
                            active = t.User.active
                        },
                    Sherbimi = getSherbimet.Where(d => d.TransaksionId == t.TransaksioniId).Select(d => d.Sherbimi).ToList()
                }).ToList();

                var rezPage = new TransaksionPage
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords,
                    TotalAmount = totalTransactionsToday,
                    MonthlyAmount = 0,
                    YearlyAmount = 0,
                    Njesite = new List<NjesiOrg>(),
                    Data = result
                };

                return Ok(ApiResponse<TransaksionPage>.Ok(rezPage, "Transactions retrieved successfully"));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing the request.", innerMessage));
            }
        }

        [HttpGet]
        [Authorize]
        [Route("ByUser")]
        [ProducesResponseType(typeof(ApiResponse<TransaksionPage>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<TransaksionPage>>> GetTransaksioninByUser(int pageNumber = 1, int pageSize = 10, int njesiaId = 0)
        {
            try
            {
                if (pageNumber <= 0 || pageSize <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Page number and page size must be greater than zero."));
                }

                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var query = _db.TransaksionParkimi
                   .AsNoTracking()
                   .Include(t => t.Njesia)
                   .Where(t => t.UserId == userId);

                var transaksionQuery = _db.TransaksionDetaj.AsNoTracking().Where(d => d.TransaksionParkimi.UserId == userId && d.TransaksionParkimi.Statusi != "Pending");

                if (njesiaId > 0)
                {
                    query = query.Where(t => t.Cilsimet.NjesiteId == njesiaId);
                    transaksionQuery = transaksionQuery.Where(d => d.TransaksionParkimi.Cilsimet.NjesiteId == njesiaId);
                }

                var orgIds = await _db.UserOrg
                    .AsNoTracking()
                    .Where(u => u.UserId == userId)
                    .Select(u => u.BiznesId)
                    .Distinct()
                    .ToListAsync();

                var njesite = await _db.NjesiOrg
                    .AsNoTracking()
                    .Where(n => orgIds.Contains(n.BiznesId) && n.active)
                    .ToListAsync();

                var totalRecords = await query.CountAsync();
                if (totalRecords == 0)
                {
                    var emptyResult = new TransaksionPage
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalPages = 0,
                        TotalRecords = 0,
                        TotalAmount = 0,
                        MonthlyAmount = 0,
                        YearlyAmount = 0,
                        Njesite = njesite,
                        Data = new List<TransaksionRead>()
                    };

                    return Ok(ApiResponse<TransaksionPage>.Ok( emptyResult, "No transactions found."));
                }

                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
                var totalAmount = await transaksionQuery.SumAsync(d => d.Cmimi);

                var now = DateTime.Now;
                var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var monthEnd = monthStart.AddMonths(1);
                var monthlyAmount = await transaksionQuery
                    .Where(d => d.TransaksionParkimi.KohaHyrjes >= monthStart && d.TransaksionParkimi.KohaHyrjes < monthEnd)
                    .SumAsync(d => d.Cmimi);

                var yearStart = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var yearEnd = yearStart.AddYears(1);
                var yearlyAmount = await transaksionQuery
                    .Where(d => d.TransaksionParkimi.KohaHyrjes >= yearStart && d.TransaksionParkimi.KohaHyrjes < yearEnd)
                    .SumAsync(d => d.Cmimi);

                var transaksionet = await query
                    .OrderByDescending(t => t.TransaksioniId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var transaksionIds = transaksionet.Select(t => t.TransaksioniId).ToList();

                var getSherbimet = await _db.TransaksionDetaj
                    .AsNoTracking()
                    .Where(c =>transaksionIds.Contains(c.TransaksionId))
                    .Include(c => c.Sherbimi)
                    .ToListAsync();

                var result = transaksionet.Select(t => new TransaksionRead
                {
                    TransaksioniId = t.TransaksioniId,
                    KohaHyrjes = t.KohaHyrjes,
                    KohaDaljes = t.KohaDaljes,
                    Cmimi = getSherbimet.Where(i => i.TransaksionId == t.TransaksioniId).Sum(c => c.Cmimi),
                    Statusi = t.Statusi,
                    Identifikues = t.Identifikues,
                    Njesia = t.Njesia,
                    Cilsimi = t.Cilsimet,
                    Useri = t.User,
                    Sherbimi = getSherbimet.Where(d => d.TransaksionId == t.TransaksioniId).Select(d => d.Sherbimi).ToList()
                }).ToList();

                var page = new TransaksionPage
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords,
                    TotalAmount = totalAmount,
                    MonthlyAmount = monthlyAmount,
                    YearlyAmount = yearlyAmount,
                    Njesite = njesite,
                    Data = result
                };

                return Ok(ApiResponse<TransaksionPage>.Ok(page, "Transactions retrieved successfully"));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing the request.", innerMessage));

            }

        }
        
        [HttpGet]
        [Authorize]
        [Route("Pending")]
        [ProducesResponseType(typeof(ApiResponse<TransaksionRead>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransaksionRead>>>> GetTransaksionPending(int? userId, int? njesiaId)
        {
            try
            {
                int orgId = int.Parse(User.FindFirst("BiznesId")!.Value);
                var query = _db.TransaksionParkimi.AsNoTracking().Where(t => t.Statusi == "Pending");

                if(userId != null) query = query.Where(t => t.UserId == userId);
                if(orgId != 0) query = query.Where(t => t.Njesia.BiznesId == orgId);
                if(njesiaId != null) query = query.Where(t => t.NjesiaId == njesiaId);

                var transaksionet = await query
                    .Include(t => t.Cilsimet)
                        .ThenInclude(c => c.Sherbimi)
                    .Include(n => n.Njesia)
                    .Include(t => t.User)
                    .ToListAsync();

                if (transaksionet.Count() == 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("No transactions found."));
                }

                var transaksionIds = transaksionet.Select(t => t.TransaksioniId).ToList();

                var getSherbimet = await _db.TransaksionDetaj.Where(c => transaksionIds.Contains(c.TransaksionId)).Include(c => c.Sherbimi).ToListAsync();

                var result = transaksionet.Select(t => new TransaksionRead
                {
                    TransaksioniId = t.TransaksioniId,
                    KohaHyrjes = t.KohaHyrjes,
                    KohaDaljes = t.KohaDaljes,
                    Cmimi = getSherbimet.Where(i => i.TransaksionId == t.TransaksioniId).Sum(c => c.Cmimi),
                    Statusi = t.Statusi,
                    Identifikues = t.Identifikues,
                    Njesia = t.Njesia,
                    Cilsimi = t.Cilsimet,
                    Useri = t.User,
                    Sherbimi = getSherbimet.Where(d => d.TransaksionId == t.TransaksioniId).Select(d => d.Sherbimi).ToList(),
                });

                return Ok(ApiResponse<IEnumerable<TransaksionRead>>.Ok(result, "Transactions retrieved successfully"));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing the request.", innerMessage));

            }

        }

        [HttpGet("{transaksioniId:int}/Price")]
        [ProducesResponseType(typeof(ApiResponse<TransaksionRead>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<TransaksionRead>>> getTransaksioninPrice(int transaksioniId)
        {
            try
            {
                var gettransaksioni = await _db.TransaksionParkimi.Where(t => t.TransaksioniId == transaksioniId).Include(c => c.Cilsimet).Include(c => c.User).Include(n => n.Njesia).FirstOrDefaultAsync(); 
                if (gettransaksioni == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Transaksioni with id {transaksioniId} not found."));
                }

                DateTime KohaDaljes = DateTime.UtcNow;
                TimeSpan difference = KohaDaljes - gettransaksioni.KohaHyrjes;
                int diff = (int)difference.TotalHours;

                var getDetajet = await _db.Detajet
                                        .Where(c => gettransaksioni.CilsimiId == c.CilsimetiId)
                                        .Where(t => (t.ToHour < diff) || (t.FromHour <= diff && t.ToHour > diff) || (t.FromHour <= diff && t.ToHour == null))
                                        .Include(c => c.CilsimetParkimit).ToListAsync();

                if (getDetajet.Count() == 0)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Detajet with cilsim id {gettransaksioni.CilsimiId} not found."));
                }
                decimal CmimiParking = 0;
                ++diff;
                foreach (var item in getDetajet)
                {
                    if (item.ToHour == null)
                    {
                        int diffDetaj = diff - item.FromHour;
                        CmimiParking += diffDetaj * item.Cmimi;
                    }
                    else if (item.ToHour < diff)
                    {
                        int diffDetaj = (int)item.ToHour - item.FromHour;
                        CmimiParking += diffDetaj * item.Cmimi;
                    }
                    else if (item.ToHour >= diff && item.FromHour < diff)
                    {
                        int diffDetaj = diff - item.FromHour;
                        CmimiParking += diffDetaj * item.Cmimi;
                    }
                }

                var getSherbimet = await _db.TransaksionDetaj.Where(t => t.TransaksionId == transaksioniId).Include(c => c.Sherbimi).ToListAsync();

                var parking = getSherbimet.Where(s => s.SherbimiId == gettransaksioni.Cilsimet.SherbimiId).FirstOrDefault();
                if (parking != null)
                {
                    parking.Cmimi = CmimiParking;
                    await _db.SaveChangesAsync();
                }

                TransaksionRead transaksionRead = new TransaksionRead
                {
                    TransaksioniId = gettransaksioni.TransaksioniId,
                    KohaDaljes = gettransaksioni.KohaDaljes,
                    KohaHyrjes = gettransaksioni.KohaHyrjes,
                    Cmimi = getSherbimet.Where(i => i.TransaksionId == transaksioniId).Sum(c => c.Cmimi),
                    Statusi = gettransaksioni.Statusi,
                    Cilsimi = gettransaksioni.Cilsimet,
                    Njesia = gettransaksioni.Njesia,
                    Useri = gettransaksioni.User,
                    Sherbimi = getSherbimet.Select(c => c.Sherbimi).ToList()
                };

                return Ok(ApiResponse<TransaksionRead>.Ok(transaksionRead, "Transaksion retrived successfully"));

            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing the request.", innerMessage));

            }

        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<TransaksionetCreateDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<TransaksionetCreateDto>>> CreateTransaksion(TransaksionetCreateDto transaksionetCreateDto)
        {
            try
            {
                int userId = transaksionetCreateDto.UserId == null ? int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value) : transaksionetCreateDto.UserId.Value;
                
                var getNjesia = await _db.NjesiOrg.Where(v => v.NjesiteId == transaksionetCreateDto.NjesiaId).FirstOrDefaultAsync();
                if (getNjesia == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Njesia with id {transaksionetCreateDto.NjesiaId} not found."));
                }
                if (getNjesia.VendeTeLira <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest($"There are no more free spaces"));
                }

                var getCilsimi = await _db.CilsimetParkimit.Where(c => c.CilsimetiId == transaksionetCreateDto.CilsimiId).Include(s => s.Sherbimi).FirstOrDefaultAsync();
                if (getCilsimi == null || getCilsimi.NjesiteId != transaksionetCreateDto.NjesiaId)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Cilsimi not Found"));
                }

                var transaksionet = _mapper.Map<TransaksionParkimi>(transaksionetCreateDto);
                transaksionet.Statusi = "Pending";
                transaksionet.KohaHyrjes = DateTime.UtcNow;
                transaksionet.KohaDaljes = null;
                transaksionet.UserId = userId;
                transaksionet.Identifikues = transaksionetCreateDto.Identifikues;

                getNjesia.VendeTeLira--;
                await _db.SaveChangesAsync();

                await _db.TransaksionParkimi.AddAsync(transaksionet);
                await _db.SaveChangesAsync();

                TransaksionDetaj transaksionParking = new TransaksionDetaj
                {
                    TransaksionId = transaksionet.TransaksioniId,
                    SherbimiId = getCilsimi.Sherbimi.SherbimiId,
                    Cmimi = 0
                };
                await _db.TransaksionDetaj.AddAsync(transaksionParking);
                await _db.SaveChangesAsync();

                var response = ApiResponse<TransaksionetCreateDto>.CreatedAt(_mapper.Map<TransaksionetCreateDto>(transaksionet), "The data has been added successfully");
                return Ok(response);

            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing the request.", innerMessage));
            }
        }
        
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<TransaksionRead>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<TransaksionRead>>> UpdateTransaktions(int id, TransaksionUpdateDto transaksionUpdateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Invalid ID supplied."));
                }
                var findTransaktion = await _db.TransaksionParkimi.Where(t=> t.TransaksioniId==id).Include(n=> n.Njesia).FirstOrDefaultAsync();
                if (findTransaktion == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Transaktion with id {id} not found."));
                }
                if (findTransaktion.Statusi == "Completed")
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Transaction is already completed"));
                }
                var sherbimiParking = await _db.CilsimetParkimit.Where(c => c.CilsimetiId == findTransaktion.CilsimiId).Include(s => s.Sherbimi).FirstOrDefaultAsync();
                if (sherbimiParking == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("There is no cilsim in this transaction"));
                }

                List<Sherbimi> getSherbimet = new List<Sherbimi>();
                if (transaksionUpdateDto.SherbimiId != null)
                {
                    getSherbimet = await _db.Sherbimi.Where(c => transaksionUpdateDto.SherbimiId.Contains(c.SherbimiId)).ToListAsync();
                }

                if (getSherbimet.Count() != 0 && getSherbimet.Any(s=> s.BiznesId != findTransaktion.Njesia.BiznesId))
                    return BadRequest(ApiResponse<object>.BadRequest("Sherbimi not found"));

                DateTime KohaDaljes = DateTime.UtcNow;
                TimeSpan difference = KohaDaljes - findTransaktion.KohaHyrjes;
                int diff = (int) difference.TotalHours;

                var getDetajet = await _db.Detajet
                                        .Where(c => findTransaktion.CilsimiId == c.CilsimetiId)
                                        .Where(t => (t.ToHour < diff) || (t.FromHour <= diff && t.ToHour > diff) || (t.FromHour <= diff && t.ToHour == null) )
                                        .Include(c => c.CilsimetParkimit).ToListAsync();

                if (getDetajet.Count == 0)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Detajet with cilsim id {findTransaktion.CilsimiId} not found."));
                }
                decimal CmimiParking = 0;
                ++diff;
                foreach (var item in getDetajet)
                {
                    if (item.ToHour == null)
                    {
                        int diffDetaj = diff - item.FromHour;
                        CmimiParking += diffDetaj * item.Cmimi;
                    }
                    else if (item.ToHour < diff)
                    {
                        int diffDetaj = (int)item.ToHour - item.FromHour;
                        CmimiParking += diffDetaj * item.Cmimi;
                    }
                    else if(item.ToHour >= diff && item.FromHour < diff)
                    {
                        int diffDetaj = diff - item.FromHour;
                        CmimiParking += diffDetaj * item.Cmimi;
                    }
                }

                findTransaktion.KohaDaljes = KohaDaljes;

                _db.TransaksionParkimi.Update(findTransaktion);
                await _db.SaveChangesAsync();

                var removeTransaksionCilsim = await _db.TransaksionDetaj.Where(c=> c.TransaksionId == id).ToListAsync();
                _db.TransaksionDetaj.RemoveRange(removeTransaksionCilsim);


                TransaksionDetaj transaksionParking = new TransaksionDetaj
                {
                    TransaksionId = findTransaktion.TransaksioniId,
                    SherbimiId = sherbimiParking.SherbimiId,
                    Cmimi = CmimiParking
                };
                await _db.TransaksionDetaj.AddAsync(transaksionParking);
                await _db.SaveChangesAsync();

                if (transaksionUpdateDto.SherbimiId != null)
                {
                    var toAddDetaje = new List<TransaksionDetaj>();
                    foreach (int i in transaksionUpdateDto.SherbimiId)
                    {
                        toAddDetaje.Add( new TransaksionDetaj
                        {
                            TransaksionId = findTransaktion.TransaksioniId,
                            SherbimiId = i,
                            Cmimi = await _db.Sherbimi.Where(s => s.SherbimiId == i).Select(c => c.Cmimi).FirstOrDefaultAsync()
                        });
                    }
                    await _db.TransaksionDetaj.AddRangeAsync(toAddDetaje);
                    await _db.SaveChangesAsync();
                }
               
                var transaksioni = _mapper.Map<TransaksionRead> (findTransaktion);
                transaksioni.Cmimi = _db.TransaksionDetaj.Where(i=> i.TransaksionId == findTransaktion.TransaksioniId).Sum(s => s.Cmimi);

                return Ok(ApiResponse<TransaksionRead>.Ok(transaksioni, "Transaksioni has been updated"));

            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing the request.",innerMessage));

            }
        }

        [HttpPut("{id:int}/Pay")]
        [ProducesResponseType(typeof(ApiResponse<TransaksionRead>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<TransaksionRead>>> PayTransaktions(int id, int? cardId)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse<object>.BadRequest("Invalid ID supplied."));
            }
            var findTransaktion = await _db.TransaksionParkimi.Where(t => t.TransaksioniId == id).Include(c => c.Cilsimet).Include(n => n.Njesia).FirstOrDefaultAsync();
            if (findTransaktion == null)
            {
                return NotFound(ApiResponse<object>.NotFound($"Transaktion with id {id} not found."));
            }
            if (findTransaktion.Statusi == "Completed")
            {
                return BadRequest(ApiResponse<object>.BadRequest($"Transaktion with id {id} is Completed."));
            }
            var getNjesia = await _db.NjesiOrg.Where(v => v.NjesiteId == findTransaktion.NjesiaId).FirstOrDefaultAsync();
            if (getNjesia == null)
            {
                return NotFound(ApiResponse<object>.NotFound("Njesia could not be found"));
            }
            
            DateTime KohaDaljes = DateTime.UtcNow;
            TimeSpan difference = KohaDaljes - findTransaktion.KohaHyrjes;
            int diff = (int)difference.TotalHours;

            var getDetajet = await _db.Detajet
                                    .Where(c => findTransaktion.CilsimiId == c.CilsimetiId)
                                    .Where(t => (t.ToHour < diff) || (t.FromHour <= diff && t.ToHour > diff) || (t.FromHour <= diff && t.ToHour == null))
                                    .Include(c => c.CilsimetParkimit).ToListAsync();

            if (getDetajet.Count == 0)
            {
                return NotFound(ApiResponse<object>.NotFound($"Detajet with cilsim id {findTransaktion.CilsimiId} not found."));
            }
            decimal CmimiParking = 0;
            ++diff;
            foreach (var item in getDetajet)
            {
                if (item.ToHour == null)
                {
                    int diffDetaj = diff - item.FromHour;
                    CmimiParking += diffDetaj * item.Cmimi;
                }
                else if (item.ToHour < diff)
                {
                    int diffDetaj = (int)item.ToHour - item.FromHour;
                    CmimiParking += diffDetaj * item.Cmimi;
                }
                else if (item.ToHour >= diff && item.FromHour < diff)
                {
                    int diffDetaj = diff - item.FromHour;
                    CmimiParking += diffDetaj * item.Cmimi;
                }
            }

            PaymentIntent? paymentIntent = null;

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try { 
                var getSherbimet = await _db.TransaksionDetaj.Where(t => t.TransaksionId == id).Include(c => c.Sherbimi).ToListAsync();
                var parking = getSherbimet.Where(s => s.SherbimiId == findTransaktion.Cilsimet.SherbimiId).FirstOrDefault();
                if (parking != null)
                {
                    parking.Cmimi = CmimiParking;
                    await _db.SaveChangesAsync();
                }

                getNjesia.VendeTeLira++;

                findTransaktion.KohaDaljes = KohaDaljes;
                findTransaktion.Statusi = "Completed";

                _db.TransaksionParkimi.Update(findTransaktion);
                await _db.SaveChangesAsync();

                if (cardId.HasValue)
                {
                    var card = await _db.CreditCards.FirstOrDefaultAsync(c => c.Id == cardId);
                    if (card == null)
                    {
                        await transaction.RollbackAsync();
                        return NotFound(ApiResponse<object>.NotFound("Card not found"));
                    }
                    if (card.UserId != findTransaktion.UserId)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(ApiResponse<object>.BadRequest($"This Card is not for this User"));
                    }
                    var finalPrice = getSherbimet.Where(i => i.TransaksionId == id).Sum(c => c.Cmimi);

                    var paymentIntentService = new PaymentIntentService();
                    paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
                    {
                        Amount = (long)(finalPrice * 100),
                        Currency = "eur",
                        Customer = card.StripeCustomerId,
                        PaymentMethod = card.StripePaymentMethodId,
                        OffSession = true,
                        Confirm = true
                    });
                    if (paymentIntent.Status != "succeeded")
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(ApiResponse<object>.BadRequest($"Payment was not completed. Stripe status: {paymentIntent.Status}"));
                    }
                }
                var transaksioni = _mapper.Map<TransaksionRead>(findTransaktion);

                await transaction.CommitAsync();
                return Ok(ApiResponse<TransaksionRead>.Ok(transaksioni, "Transaksioni has been updated"));

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                if (paymentIntent?.Status == "succeeded")
                {
                    try
                    {
                        var refundService = new RefundService();
                        await refundService.CreateAsync(new RefundCreateOptions { PaymentIntent = paymentIntent.Id });
                    }
                    catch
                    {
                        return StatusCode(500, ApiResponse<object>.Error(500, "Failed To Refund"));
                    }
                }
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing the request.", innerMessage));

            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Manager, Admin , Super Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeteleTransaction(int id)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse<object>.BadRequest("Invalid ID supplied."));
            }
            await using var dbTransaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var transaction = await _db.TransaksionParkimi.FindAsync(id);
                if (transaction == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Transaction with id {id} does not exist"));
                }
                else if(transaction.Statusi == "Completed")
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Transaction is already Payed"));
                }
                
                var details = await _db.TransaksionDetaj.Where(t => t.TransaksionId == id).ToListAsync();
                if (details != null)
                {
                    _db.TransaksionDetaj.RemoveRange(details);
                }

                _db.TransaksionParkimi.Remove(transaction);
                await _db.SaveChangesAsync();

                var getNjesia = await _db.NjesiOrg.Where(v => v.NjesiteId == transaction.NjesiaId).FirstOrDefaultAsync();
                if (getNjesia == null)
                {
                    return NotFound(ApiResponse<object>.NotFound("Njesia could not be found"));
                }
                getNjesia.VendeTeLira++;
                await _db.SaveChangesAsync();

                await dbTransaction.CommitAsync();
                return Ok(ApiResponse<object>.NoContent($"Transaction with ID {id} has been deleted."));
            }
            catch (Exception ex) 
            {
                await dbTransaction.RollbackAsync();
                var errorResponse = ApiResponse<object>.Error(500, $"An Error Occurred while deleting Transaction: ", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
    }
}
