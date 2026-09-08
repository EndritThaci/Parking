using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking_project.Data;
using Parking_project.Models;
using Parking_project.Models.DTO;

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
