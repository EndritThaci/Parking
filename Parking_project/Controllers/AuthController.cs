using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking_project.Data;
using Parking_project.Models;
using Parking_project.Models.DTO;
using Parking_project.Services;

namespace Parking_project.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AplicationDbContext _db;
        public AuthController(IAuthService authService, AplicationDbContext db)
        {
            _authService = authService;
            _db = db;
        }


        [HttpPost]
        [Route("signUp")]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status409Conflict)] 
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserReadDTO>>> Register([FromBody]UserCreateDTO userDTO)
        {
            try
            {
                if (userDTO == null)
                {
                    return BadRequest(ApiResponse<UserReadDTO>.BadRequest("Data is required"));
                }

                if (await _authService.UsernameExistsAsync(userDTO.Username))
                {
                    return Conflict(ApiResponse<UserReadDTO>.Conflict("Username already exists"));
                }

                var user = await _authService.RegisterAsync(userDTO, "Customer");
                if (user == null)
                {
                    return BadRequest(ApiResponse<UserReadDTO>.BadRequest("Registration failed"));
                }

                var allowedBiznesIds = await _db.Organizata
                        .Where(o => o.AllowCustomers)
                        .Select(o => o.BiznesId)
                        .ToListAsync();

                var newUserOrgs = allowedBiznesIds
                    .Select(biznesId => new UserOrg
                    {
                        UserId = user.UserId,
                        BiznesId = biznesId,
                        NjesiaId = null
                    })
                    .ToList();

                if (newUserOrgs.Any())
                {
                    await _db.UserOrg.AddRangeAsync(newUserOrgs);
                }

                await _db.SaveChangesAsync();

                var response = ApiResponse<UserReadDTO>.CreatedAt(user, "User created successfully");
                return CreatedAtAction(nameof(Register), response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<UserReadDTO>.Error(500, "An Error Occurred while registering", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost]
        [Route("signUp/Admin")]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserReadDTO>>> RegisterAdmin([FromBody] UserCreateDTO userDTO)
        {
            try
            {
                if (userDTO == null)
                {
                    return BadRequest(ApiResponse<UserReadDTO>.BadRequest("Data is required"));
                }

                if (await _authService.UsernameExistsAsync(userDTO.Username))
                {
                    return Conflict(ApiResponse<UserReadDTO>.Conflict("Username already exists"));
                }

                if (userDTO.UserOrgs != null && userDTO.UserOrgs.Count > 1)
                {
                    return BadRequest(ApiResponse<UserReadDTO>.BadRequest("Admin can have only one organization"));
                }

                var user = await _authService.RegisterAsync(userDTO, "Admin");
                if (user == null)
                {
                    return BadRequest(ApiResponse<UserReadDTO>.BadRequest("Registration failed"));
                }

                var response = ApiResponse<UserReadDTO>.CreatedAt(user, "User created successfully");
                return CreatedAtAction(nameof(Register), response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<UserCreateDTO>.Error(500, "An Error Occurred while registering", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost]
        [Route("signUp/Manager")]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserReadDTO>>> RegisterManager([FromBody] UserCreateDTO userDTO)
        {
            try
            {
                if (userDTO == null)
                {
                    return BadRequest(ApiResponse<UserReadDTO>.BadRequest("Data is required"));
                }

                if (await _authService.UsernameExistsAsync(userDTO.Username))
                {
                    return Conflict(ApiResponse<UserReadDTO>.Conflict("Username already exists"));
                }

                if (userDTO.UserOrgs != null && userDTO.UserOrgs.Count > 1)
                {
                    return BadRequest(ApiResponse<UserReadDTO>.BadRequest("Manager can have only one organization"));
                }

                var user = await _authService.RegisterAsync(userDTO, "Manager");
                if (user == null)
                {
                    return BadRequest(ApiResponse<UserReadDTO>.BadRequest("Registration failed"));
                }

                var response = ApiResponse<UserReadDTO>.CreatedAt(user, "User created successfully");
                return CreatedAtAction(nameof(Register), response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<UserCreateDTO>.Error(500, "An Error Occurred while registering", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
        
        [HttpPost]
        [Route("signUp/Employee")]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserReadDTO>>> RegisterEmployee([FromBody] UserCreateDTO userDTO)
        {
            try
            {
                if (userDTO == null)
                {
                    return BadRequest(ApiResponse<UserReadDTO>.BadRequest("Data is required"));
                }

                if (await _authService.UsernameExistsAsync(userDTO.Username))
                {
                    return Conflict(ApiResponse<UserReadDTO>.Conflict("Username already exists"));
                }

                if (userDTO.UserOrgs != null && userDTO.UserOrgs.Count > 1)
                {
                    return BadRequest(ApiResponse<UserReadDTO>.BadRequest("Employee can have only one organization"));
                }

                var user = await _authService.RegisterAsync(userDTO, "Employee");
                if (user == null)
                {
                    return BadRequest(ApiResponse<UserReadDTO>.BadRequest("Registration failed"));
                }

                var response = ApiResponse<UserReadDTO>.CreatedAt(user, "User created successfully");
                return CreatedAtAction(nameof(Register), response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<UserCreateDTO>.Error(500, "An Error Occurred while registering", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost]
        [Route("logIn")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDTO>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDTO>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                if (loginDTO == null)
                {
                    return BadRequest(ApiResponse<LoginResponseDTO>.BadRequest("Data is required"));
                }


                var login = await _authService.LoginAsync(loginDTO);
                if (login == null || login.UserReadDTO == null || login.Token == null)
                {
                    return BadRequest(ApiResponse<LoginResponseDTO>.BadRequest("Login failed"));
                }

                var response = ApiResponse<LoginResponseDTO>.Ok(login, "Login successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<LoginResponseDTO>.Error(500, "An Error Occurred while Logging in", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
    }
}
