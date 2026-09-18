using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking_project.Data;
using Parking_project.Migrations;
using Parking_project.Models;
using Parking_project.Models.DTO;
using Parking_project.Services;
using System.Security.Claims;

namespace Parking_project.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UseriController : ControllerBase
    {
        private readonly AplicationDbContext _db;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public UseriController(AplicationDbContext db, IMapper mapper, IAuthService authService)
        {
            _db = db;
            _mapper = mapper;
            _authService = authService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserReadDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserReadDTO>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserReadDTO>>>> GetUsers()
        {
            try
            {
                var users = await _db.Useri
                    .Where(u => u.active)
                    .Include(u => u.UserOrgs)
                        .ThenInclude(uo => uo.Organizata)
                    .Include(u => u.UserOrgs)
                        .ThenInclude(uo => uo.Njesi)
                    .ToListAsync();

                var data = _mapper.Map<IEnumerable<UserReadDTO>>(users);

                return Ok(ApiResponse<IEnumerable<UserReadDTO>>.Ok(data,"Users retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse =ApiResponse<IEnumerable<UserReadDTO>>.Error(500, "An error occurred while retrieving users",ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<Useri>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Useri>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<Useri>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserReadDTO>>> GetUserById(int id)
        {
            try
            {
                if (id <= 0)
                    return NotFound(ApiResponse<UserReadDTO>.NotFound("Invalid ID"));

                var user = await _db.Useri.AsNoTracking().Where(u => u.UserId == id && u.active)
                    .Include(uo => uo.UserOrgs)
                        .ThenInclude(o => o.Organizata)
                    .Include(uo => uo.UserOrgs)
                        .ThenInclude(n => n.Njesi)
                    .FirstOrDefaultAsync();
                if (user == null)
                    return NotFound(ApiResponse<UserReadDTO>.NotFound($"User with ID {id} not found"));

                var data = _mapper.Map<UserReadDTO>(user);

                return Ok(ApiResponse<UserReadDTO>.Ok(data, "User retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<UserReadDTO>.Error(500, "An Error Occurred while retrieving User", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
        
        [HttpGet("{id:int}/Totals")]
        [ProducesResponseType(typeof(ApiResponse<UserTotalsDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserTotalsDTO>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<UserTotalsDTO>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserTotalsDTO>>> GetUserTotalsById(int id, int? orgId)
        {
            try
            {
                if (id <= 0)
                    return NotFound(ApiResponse<UserTotalsDTO>.NotFound("Invalid ID"));

                //User
                var userQuery = _db.Useri.AsNoTracking().Where(u => u.UserId == id && u.active);
                if (orgId.HasValue) userQuery = userQuery.Where(u => u.UserOrgs.Any(uo => uo.BiznesId == orgId.Value));

                var user = await userQuery.FirstOrDefaultAsync();
                if (user == null)
                    return NotFound(ApiResponse<UserTotalsDTO>.NotFound($"User with ID {id} not found"));

                //Dates
                var now = DateTime.Now;
                var todayStart = now.Date;
                var tomorrowStart = todayStart.AddDays(1);

                int daysSinceMonday = ((int)now.DayOfWeek + 6) % 7;
                var weekStart = todayStart.AddDays(-daysSinceMonday);
                var weekEnd = weekStart.AddDays(7);

                var monthStart = new DateTime(now.Year, now.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                var yearStart = new DateTime( now.Year, 1, 1);
                var yearEnd = yearStart.AddYears(1);

                //Transactions
                var transactionQuery = _db.TransaksionParkimi.AsNoTracking().Where(t => t.UserId == id && t.Statusi == "Completed");
                if (orgId.HasValue) transactionQuery = transactionQuery.Where(t => t.Njesia.BiznesId == orgId.Value);

                var transaksionet = await transactionQuery
                    .Include(t => t.Cilsimet)
                        .ThenInclude(c => c.Sherbimi)
                    .Include(n => n.Njesia)
                    .Include(t => t.User)
                    .ToListAsync();

                var transaksionIds = transaksionet.Select(t => t.TransaksioniId).ToList();

                var getSherbimet = await _db.TransaksionDetaj.Where(c => transaksionIds.Contains(c.TransaksionId)).Include(c => c.Sherbimi).ToListAsync();
                var transactions = transaksionet.Select(t => new TransaksionRead
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
                    })
                    .ToList();

                var result = new UserTotalsDTO
                {
                    User = _mapper.Map<UserReadDTO>(user)
                };

                //Totals
                var today = transactions
                    .Where(t => t.KohaHyrjes >= todayStart && t.KohaHyrjes < tomorrowStart)
                    .ToList();

                var week = transactions
                    .Where(t => t.KohaHyrjes >= weekStart && t.KohaHyrjes < weekEnd)
                    .ToList();

                var month = transactions
                    .Where(t => t.KohaHyrjes >= monthStart && t.KohaHyrjes < monthEnd)
                    .ToList();

                var year = transactions
                    .Where(t => t.KohaHyrjes >= yearStart && t.KohaHyrjes < yearEnd)
                    .ToList();

                result.Total = new TransactionTotalsDTO
                {
                    CountToday = today.Count,
                    AmountToday = today.Sum(t => t.Cmimi ?? 0m),
                    CountWeek = week.Count,
                    AmountWeek = week.Sum(t => t.Cmimi ?? 0m),
                    CountMonth = month.Count,
                    AmountMonth = month.Sum(t => t.Cmimi ?? 0m),
                    CountYear = year.Count,
                    AmountYear = year.Sum(t => t.Cmimi ?? 0m),
                    CountAll = transactions.Count,
                    AmountAll = transactions.Sum(t => t.Cmimi ?? 0m)
                };

                return Ok(ApiResponse<UserTotalsDTO>.Ok(result, "User totals retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<UserTotalsDTO>.Error(500, "An Error Occurred while retrieving User", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
        
        [HttpGet("Pagination")]
        [ProducesResponseType(typeof(ApiResponse<List<Useri>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Useri>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<Useri>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<Useri>>>> GetUsersByOrgId(int? orgId, string? search, string? role, bool active, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _db.Useri.AsNoTracking().Where(u => u.Role != "Super Admin");

                if(orgId > 0) query = query.Where(u => u.UserOrgs.Any(x=> x.BiznesId == orgId));
                if(active) query = query.Where(u => u.active);
                if (!string.IsNullOrWhiteSpace(role)) query = query.Where(u => u.Role == role.Trim());
                
                // Search
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(u => u.Emri.ToLower().Contains(search) || u.Mbiemri.ToLower().Contains(search) || u.Email.ToLower().Contains(search));
                }

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double) pageSize);

                var users = await query
                    .OrderBy(u => u.UserId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = new UserPage
                {
                    Data = users,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalCount = totalCount
                };

                return Ok(ApiResponse<UserPage>.Ok(result, "Users retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<UserPage>.Error(500, "An Error Occurred while retrieving User", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPut("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserUpdateDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserUpdateDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<UserUpdateDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UserUpdateDTO>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<UserUpdateDTO>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserUpdateDTO>>> UpdateUser(int id, UserUpdateDTO dto)
        {
            try
            {
                if (dto == null || id != dto.UserId)
                    return BadRequest(ApiResponse<UserUpdateDTO>.BadRequest("Invalid data"));

                var user = await _db.Useri.Include(u => u.UserOrgs).FirstOrDefaultAsync(u => u.UserId == id && u.active);
                if (user == null)
                    return NotFound(ApiResponse<UserUpdateDTO>.NotFound($"User with ID {id} not found"));

                user.Emri = dto.Emri;
                user.Mbiemri = dto.Mbiemri;

                if (dto.UserOrgs != null)
                {
                    var biznesIds = dto.UserOrgs
                        .Select(x => x.BiznesId)
                        .Distinct()
                        .ToList();

                    var njesiIds = dto.UserOrgs
                        .Where(x => x.NjesiaId.HasValue)
                        .Select(x => x.NjesiaId!.Value)
                        .Distinct()
                        .ToList();

                    var existingBiznesIds = await _db.Organizata
                        .Where(o => biznesIds.Contains(o.BiznesId))
                        .Select(o => o.BiznesId)
                        .ToListAsync();

                    var missingBiznesIds = biznesIds
                        .Except(existingBiznesIds)
                        .ToList();

                    if (missingBiznesIds.Any())
                    {
                        return NotFound(ApiResponse<UserUpdateDTO>.NotFound( $"Organization(s) not found: {string.Join(", ", missingBiznesIds)}"));
                    }

                    var existingNjesiIds = await _db.NjesiOrg
                        .Where(n => njesiIds.Contains(n.NjesiteId))
                        .Select(n => n.NjesiteId)
                        .ToListAsync();

                    var missingNjesiIds = njesiIds
                        .Except(existingNjesiIds)
                        .ToList();

                    if (missingNjesiIds.Any())
                    {
                        return NotFound(ApiResponse<UserUpdateDTO>.NotFound( $"Unit(s) not found: {string.Join(", ", missingNjesiIds)}"));
                    }


                    if (user.UserOrgs != null && user.UserOrgs.Any())
                    {
                        _db.UserOrg.RemoveRange(user.UserOrgs);
                    }

                    var newUserOrgs = dto.UserOrgs
                        .Select(x => new UserOrg
                        {
                            UserId = user.UserId,
                            BiznesId = x.BiznesId,
                            NjesiaId = x.NjesiaId
                        })
                        .ToList();

                    if (newUserOrgs.Any())
                    {
                        await _db.UserOrg.AddRangeAsync(newUserOrgs);
                    }
                }

                await _db.SaveChangesAsync();

                return Ok(ApiResponse<UserUpdateDTO>.Ok(dto, "User updated successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<UserUpdateDTO>.Error(500, "An Error Occurred while editing user", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserReadDTO>>> DeleteUser(int id)
        {
            try
            {
                var user = await _db.Useri.FirstOrDefaultAsync(u => u.UserId == id);
                if (user == null)
                    return NotFound(ApiResponse<UserReadDTO>.NotFound($"User with ID {id} not found"));

                user.active = false;
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<UserReadDTO>.NoContent("User deleted successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<UserReadDTO>.Error(500, "An Error Occurred while deleting User", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPut("Password")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<Useri>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Useri>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Useri>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<Useri>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<Useri>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<Useri>>> ChangePassword(ChangePasswordDTO dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _db.Useri.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                    return NotFound(ApiResponse<Useri>.NotFound($"User with ID {userId} not found"));

                if (dto.NewPassword != dto.ConfirmPassword)
                    return BadRequest(ApiResponse<Useri>.BadRequest("New password and confirmation do not match"));

                if (dto.NewPassword == dto.OldPassword)
                    return BadRequest(ApiResponse<Useri>.BadRequest("New password and Old password can not match"));

                var response = await _authService.ChangePassword(user, dto.OldPassword, dto.NewPassword);
                if (string.IsNullOrEmpty(response))
                    return BadRequest(ApiResponse<Useri>.BadRequest("Incorrect Password"));

                user.Passwordi = response;

                await _db.SaveChangesAsync();

                return Ok(ApiResponse<Useri>.Ok(user, "User updated successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<Useri>.Error(500, "An Error Occurred while editing user", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPut("{orgId:int}/ActivateSuperAdmin")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<string>>> ActivateSuperAdmin(int orgId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _db.Useri.Include(u => u.UserOrgs).FirstOrDefaultAsync(u => u.UserId == userId && u.active);
                if (user == null)
                    return NotFound(ApiResponse<string>.NotFound($"User with ID {userId} not found"));

                var org = await _db.Organizata.FirstOrDefaultAsync(o => o.BiznesId == orgId);
                if (org == null)
                    return NotFound(ApiResponse<string>.NotFound($"Organization with ID {orgId} not found"));

                var existingUserOrg = user.UserOrgs?.FirstOrDefault(x => x.BiznesId == orgId);

                if (existingUserOrg != null)
                {
                    _db.UserOrg.Remove(existingUserOrg);
                }
                else
                {
                    var newUserOrg = new UserOrg
                    {
                        UserId = user.UserId,
                        BiznesId = orgId
                    };

                    await _db.UserOrg.AddAsync(newUserOrg);
                }

                await _db.SaveChangesAsync();

                var token = _authService.GenerateToken(user);

                return Ok(ApiResponse<string>.Ok(token, "User updated successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<Useri>.Error(500, "An Error Occurred while editing user", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
    }
}