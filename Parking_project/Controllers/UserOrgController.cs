using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking_project.Data;
using Parking_project.Models;
using Parking_project.Models.DTO;
using System.Security.Claims;

namespace Parking_project.Controllers
{
    [ApiController]
    [Route("api/userOrg")]
    public class UserOrgController : ControllerBase
    {
        private readonly AplicationDbContext _db;
        private readonly IMapper _mapper;

        public UserOrgController(AplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet("User/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<List<UserOrg>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserOrg>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<UserOrg>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<UserOrg>>>> GetUserOrgByUserId(int id)
        {
            try
            {
                if (id <= 0)
                    return NotFound(ApiResponse<UserOrg>.NotFound("Invalid ID"));

                var user = await _db.UserOrg.AsNoTracking().Where(u => u.UserId == id && u.User.active)
                    .Include(o => o.Organizata)
                    .Include(n => n.Njesi)
                    .ToListAsync();
                if (user == null || !user.Any())
                    return NotFound(ApiResponse<UserOrg>.NotFound($"User with ID {id} not found"));

                return Ok(ApiResponse<List<UserOrg>>.Ok(user, "User retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<UserOrg>.Error(500, "An Error Occurred while retrieving User", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("Njesi")]
        [ProducesResponseType(typeof(ApiResponse<UserOrg>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserOrg>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UserOrg>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<UserOrg>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserOrg>>> CreateUserOrgByNjesia(int userId, int njesiaId)
        {
            try
            {
                if (userId <= 0 || njesiaId <= 0)
                    return BadRequest(ApiResponse<UserOrg>.BadRequest("Invalid ID"));

                var njesia = await _db.NjesiOrg.FindAsync(njesiaId);
                if(njesia == null)
                    return NotFound(ApiResponse<UserOrg>.NotFound("Njesi not found"));

                var user = await _db.UserOrg.Where(u => u.UserId == userId && u.BiznesId == njesia.BiznesId).FirstOrDefaultAsync();
                if (user != null)
                    return Ok(ApiResponse<UserOrg>.Ok(user, "UserOrg already exists"));

                var newUserOrg = new UserOrg
                {
                    UserId = userId,
                    BiznesId = njesia.BiznesId
                };

                await _db.UserOrg.AddAsync(newUserOrg);
                return Ok(ApiResponse<UserOrg>.Ok(newUserOrg, "UserOrg created successfully successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<UserOrg>.Error(500, "An Error Occurred while retrieving User", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPut("All")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserOrg?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserOrg?>>> UpdateAllUserOrgs(bool add) //if add=true add all orgs else remove all
        {
            try
            {
                var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdValue, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.Error(401, "Invalid user identity"));
                }

                var user = await _db.Useri.Include(u => u.UserOrgs).FirstOrDefaultAsync(u => u.UserId == userId && u.active);
                if (user == null)
                    return NotFound(ApiResponse<object>.NotFound($"User with ID {userId} not found"));

                var existingUserOrgs = await _db.UserOrg
                    .Where(uo => uo.UserId == userId)
                    .ToListAsync();

                if (!add)
                {
                    if (existingUserOrgs.Any())
                    {
                        _db.UserOrg.RemoveRange(existingUserOrgs);
                    }
                }
                else
                {
                    var allowedBiznesIds = await _db.Organizata
                        .Where(o => o.AllowCustomers)
                        .Select(o => o.BiznesId)
                        .ToListAsync();

                    var existingBiznesIds = existingUserOrgs
                        .Select(uo => uo.BiznesId)
                        .ToHashSet();

                    var newUserOrgs = allowedBiznesIds
                        .Where(biznesId => !existingBiznesIds.Contains(biznesId))
                        .Select(biznesId => new UserOrg
                        {
                            UserId = userId,
                            BiznesId = biznesId,
                            NjesiaId = null
                        })
                        .ToList();

                    if (newUserOrgs.Any())
                    {
                        await _db.UserOrg.AddRangeAsync(newUserOrgs);
                    }
                }

                await _db.SaveChangesAsync();

                return Ok(ApiResponse<UserOrg?>.Ok(null, "UserOrgs updated successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An Error Occurred while editing userOrgs", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpDelete("User/{userId:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUserOrg(int userId)
        {
            try
            {
                var biznesId = int.Parse(User.FindFirst("BiznesId")!.Value);
                var userOrg = await _db.UserOrg.FirstOrDefaultAsync(u => u.UserId == userId && u.BiznesId == biznesId);
                if (userOrg == null) return NotFound(ApiResponse<object>.NotFound($"User with ID {userId} not found"));

                _db.UserOrg.Remove(userOrg);
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<object>.NoContent("User remuved from Organisation successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An Error Occurred while deleting User", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

    }
}
