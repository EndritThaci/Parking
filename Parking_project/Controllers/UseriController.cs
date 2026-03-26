using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking_project.Data;
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
                var users = await _db.Useri.Where(u => u.active).ToListAsync();
                var data = _mapper.Map<IEnumerable<UserReadDTO>>(users);

                return Ok(ApiResponse<IEnumerable<UserReadDTO>>.Ok(data, "Users retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<IEnumerable<UserReadDTO>>.Error(500, "An Error Occurred while retrieving User", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<Useri>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<Useri>>> GetUserById(int id)
        {
            try
            {
                if (id <= 0)
                    return NotFound(ApiResponse<UserReadDTO>.NotFound("Invalid ID"));

                var user = await _db.Useri.Where(u => u.UserId == id && u.active).Include(o => o.Organizata).Include(n => n.Njesi).FirstOrDefaultAsync();
                if (user == null)
                    return NotFound(ApiResponse<UserReadDTO>.NotFound($"User with ID {id} not found"));

                return Ok(ApiResponse<Useri>.Ok(user, "User retrieved successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<UserReadDTO>.Error(500, "An Error Occurred while retrieving User", ex.Message);
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

                var user = await _db.Useri.Where(u => u.active).FirstOrDefaultAsync(u => u.UserId == id);
                if (user == null)
                    return NotFound(ApiResponse<UserUpdateDTO>.NotFound($"User with ID {id} not found"));

                var organizataExists = await _db.Organizata.AnyAsync(o => o.BiznesId == dto.BiznesId);
                if (!organizataExists)
                    return NotFound(ApiResponse<UserUpdateDTO>.NotFound("Organizata not found"));

                _mapper.Map(dto, user);
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

    }
}