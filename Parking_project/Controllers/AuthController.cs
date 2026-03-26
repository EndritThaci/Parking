using AutoMapper;
using Parking_project.Data;
using Parking_project.Models;
using Parking_project.Models.DTO;
using Parking_project.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Parking_project.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
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

                if (await _authService.IsEmailExistsAsync(userDTO.Email))
                {
                    return Conflict(ApiResponse<UserReadDTO>.Conflict("Email already exists"));
                }

                var user = await _authService.RegisterAsync(userDTO, "Customer");
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

                if (await _authService.IsEmailExistsAsync(userDTO.Email))
                {
                    return Conflict(ApiResponse<UserReadDTO>.Conflict("Email already exists"));
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

                if (await _authService.IsEmailExistsAsync(userDTO.Email))
                {
                    return Conflict(ApiResponse<UserReadDTO>.Conflict("Email already exists"));
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

        [HttpPost]
        [Route("signUp/SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<UserReadDTO>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserReadDTO>>> RegisterSuperAdmin([FromBody] UserCreateDTO userDTO)
        {
            try
            {
                if (userDTO == null)
                {
                    return BadRequest(ApiResponse<UserReadDTO>.BadRequest("Data is required"));
                }

                if (await _authService.IsEmailExistsAsync(userDTO.Email))
                {
                    return Conflict(ApiResponse<UserReadDTO>.Conflict("Email already exists"));
                }

                var user = await _authService.RegisterAsync(userDTO, "Super Admin");
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

    }
}
