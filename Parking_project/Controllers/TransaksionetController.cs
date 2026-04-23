using AutoMapper;
using Parking_project.Data;
using Parking_project.Models;
using Parking_project.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
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
        [Route("ByOrg")]
        [ProducesResponseType(typeof(ApiResponse<TransaksionRead>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransaksionRead>>>> GetTransaksioninByOrg()
        {
            try
            {
                int orgId = int.Parse(User.FindFirst("BiznesId")!.Value);

                var transaksionet = await _db.TransaksionParkimi
                    .Include(t => t.Cilsimet)
                        .ThenInclude(c => c.Sherbimi)
                    .Include(n=> n.Njesia)
                    .Include(t => t.User)
                    .Where(t => t.Cilsimet.Sherbimi.BiznesId == orgId)
                    .ToListAsync();

                var transaksionIds = transaksionet.Select(t => t.TransaksioniId).ToList();

                var getSherbimet = await _db.TransaksionDetaj.Where(c => transaksionIds.Contains(c.TransaksionId)).Include(c => c.Sherbimi).ToListAsync();

                if (transaksionet.Count() == 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("No transactions found."));
                }

                var result = transaksionet.Select(t => new TransaksionRead
                {
                    TransaksioniId = t.TransaksioniId,
                    KohaHyrjes = t.KohaHyrjes,
                    KohaDaljes = t.KohaDaljes,
                    Cmimi = getSherbimet.Where(i => i.TransaksionId == t.TransaksioniId).Sum(c => c.Cmimi),
                    Statusi = t.Statusi,
                    Njesia = t.Njesia,
                    Cilsimi = t.Cilsimet,
                    Useri = t.User,
                    Sherbimi = getSherbimet.Where(d => d.TransaksionId == t.TransaksioniId).Select(d => d.Sherbimi).ToList(),
                });

                foreach (var rez in result)
                {
                    rez.Useri.Passwordi = "";
                }

                return Ok(ApiResponse<IEnumerable<TransaksionRead>>.Ok(result, "Transactions retrieved successfully"));
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
        [ProducesResponseType(typeof(ApiResponse<TransaksionRead>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransaksionRead>>>> GetTransaksioninByNjesi()
        {
            try
            {
                int njeisaId = int.Parse(User.FindFirst("NjesiaId")!.Value);

                var transaksionet = await _db.TransaksionParkimi
                    .Include(t => t.Cilsimet)
                        .ThenInclude(c => c.Sherbimi)
                    .Include(n=> n.Njesia)
                    .Include(t => t.User)
                    .Where(t => t.Cilsimet.NjesiteId == njeisaId)
                    .ToListAsync();

                var transaksionIds = transaksionet.Select(t => t.TransaksioniId).ToList();

                var getSherbimet = await _db.TransaksionDetaj.Where(c => transaksionIds.Contains(c.TransaksionId)).Include(c => c.Sherbimi).ToListAsync();

                if (transaksionet.Count() == 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("No transactions found."));
                }

                var result = transaksionet.Select(t => new TransaksionRead
                {
                    TransaksioniId = t.TransaksioniId,
                    KohaHyrjes = t.KohaHyrjes,
                    KohaDaljes = t.KohaDaljes,
                    Cmimi = getSherbimet.Where(i => i.TransaksionId == t.TransaksioniId).Sum(c => c.Cmimi),
                    Statusi = t.Statusi,
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

        [HttpGet]
        [Authorize]
        [Route("ByUser")]
        [ProducesResponseType(typeof(ApiResponse<TransaksionRead>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransaksionRead>>>> GetTransaksioninByUser()
        {
            try
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var transaksionet = await _db.TransaksionParkimi
                    .Include(t => t.Cilsimet)
                        .ThenInclude(c => c.Sherbimi)
                    .Include(n => n.Njesia)
                    .Include(t => t.User)
                    .Where(t => t.UserId == userId)
                    .ToListAsync();

                var transaksionIds = transaksionet.Select(t => t.TransaksioniId).ToList();

                var getSherbimet = await _db.TransaksionDetaj.Where(c => transaksionIds.Contains(c.TransaksionId)).Include(c => c.Sherbimi).ToListAsync();

                if (transaksionet.Count() == 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("No transactions found."));
                }

                var result = transaksionet.Select(t => new TransaksionRead
                {
                    TransaksioniId = t.TransaksioniId,
                    KohaHyrjes = t.KohaHyrjes,
                    KohaDaljes = t.KohaDaljes,
                    Cmimi = getSherbimet.Where(i => i.TransaksionId == t.TransaksioniId).Sum(c => c.Cmimi),
                    Statusi = t.Statusi,
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

                gettransaksioni.KohaDaljes = DateTime.UtcNow;
                _db.TransaksionParkimi.Update(gettransaksioni);
                await _db.SaveChangesAsync();

                var getSherbimet = await _db.TransaksionDetaj.Where(t => t.TransaksionId == transaksioniId).Include(c => c.Sherbimi).ToListAsync();

                var parking = getSherbimet.Where(s => s.SherbimiId == gettransaksioni.Cilsimet.SherbimiId).FirstOrDefault();
                if (parking != null)
                {
                    parking.Cmimi = CmimiParking;

                    _db.TransaksionDetaj.Update(parking);
                    await _db.SaveChangesAsync();
                }


                List<Sherbimi> getSherbim = getSherbimet.Select(c => c.Sherbimi).ToList();

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

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<TransaksionetCreateDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<TransaksionetCreateDto>>> CreateTransaksion(TransaksionetCreateDto transaksionetCreateDto)
        {
            try
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
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

                if (getDetajet.Count() == 0)
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
                    foreach (int i in transaksionUpdateDto.SherbimiId)
                    {
                        TransaksionDetaj transaksionDetaj = new TransaksionDetaj
                        {
                            TransaksionId = findTransaktion.TransaksioniId,
                            SherbimiId = i,
                            Cmimi = await _db.Sherbimi.Where(s => s.SherbimiId == i).Select(c => c.Cmimi).FirstOrDefaultAsync()
                        };


                        await _db.TransaksionDetaj.AddAsync(transaksionDetaj);
                        await _db.SaveChangesAsync();
                    }
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
        public async Task<ActionResult<ApiResponse<TransaksionRead>>> PayTransaktions(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Invalid ID supplied."));
                }
                var findTransaktion = await _db.TransaksionParkimi.Where(t => t.TransaksioniId == id).Include(n => n.Njesia).FirstOrDefaultAsync();
                if (findTransaktion == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Transaktion with id {id} not found."));
                }

                var getNjesia = await _db.NjesiOrg.Where(v => v.NjesiteId == findTransaktion.NjesiaId).FirstOrDefaultAsync();
                if (getNjesia == null)
                {
                    return NotFound(ApiResponse<object>.NotFound("Njesia could not be found"));
                }
                getNjesia.VendeTeLira++;

                await _db.SaveChangesAsync();

                findTransaktion.Statusi = "Completed";

                _db.TransaksionParkimi.Update(findTransaktion);
                await _db.SaveChangesAsync();

                var transaksioni = _mapper.Map<TransaksionRead>(findTransaktion);

                return Ok(ApiResponse<TransaksionRead>.Ok(transaksioni, "Transaksioni has been updated"));

            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, ApiResponse<object>.Error(500, "An error occurred while processing the request.", innerMessage));

            }
        }

    }
}
