using AutoMapper;
using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Parking_web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly INjesiaService _njesiaService;
        private readonly IOrganizataService _orgService;
        private readonly IMapper _mapper;

        public AuthController(IAuthService authService, IMapper mapper, INjesiaService njesiaService, IOrganizataService orgService)
        {
            _authService = authService;
            _mapper = mapper;
            _njesiaService = njesiaService;
            _orgService = orgService;
        }

        [HttpGet]
        public IActionResult LogIn()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogIn(LoginDTO loginDTO)
        {
            try
            {
                var response = await _authService.LoginAsync<ApiResponse<LoginResponseDTO>>(loginDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    LoginResponseDTO model = response.Data;

                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(model.Token);

                    var identety = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    identety.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? string.Empty));
                    identety.AddClaim(new Claim("Emri", jwt.Claims.FirstOrDefault(c => c.Type == "emri")?.Value ?? string.Empty));
                    identety.AddClaim(new Claim(ClaimTypes.Role, jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? string.Empty));
                    identety.AddClaim(new Claim("BiznesId", jwt.Claims.FirstOrDefault(c => c.Type == "BiznesId")?.Value ?? string.Empty));
                    identety.AddClaim(new Claim("NjesiaId", jwt.Claims.FirstOrDefault(c => c.Type == "NjesiaId")?.Value ?? string.Empty));
                    var principal = new ClaimsPrincipal(identety);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    HttpContext.Session.SetString(SD.SessionToken, model.Token!);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["error"] = "Login deshtoj. Ju lutem provoni perseri.";
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View();
        }

        private async Task PopulateOrgViewBag()
        {
            var orgResponse = await _orgService.GetAllAsync<ApiResponse<List<Organizata>>>();
            var organizatat = orgResponse?.Data;

            ViewBag.OrgList = new SelectList(organizatat, "BiznesId", "EmriBiznesit") ?? new SelectList("");
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            await PopulateOrgViewBag();
            return View(new UserCreateDTO
            {
                Email = string.Empty,
                Emri = string.Empty,
                Passwordi = string.Empty,
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserCreateDTO userDTO)
        {
            try
            {
                userDTO.NjesiaId = null;
                ApiResponse<UserReadDTO>? response = await _authService.RegisterAsync<ApiResponse<UserReadDTO>>(userDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Regjistrimi u be me sukses! Ju lutem shtypni te dhenat tuaja.";
                    return RedirectToAction("LogIn");
                }
                else
                {

                    TempData["error"] = response?.Message ?? "Regjistrimi deshtoj. Ju lutem provoni perseri.";
                    await PopulateOrgViewBag();
                    return View(userDTO);
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            await PopulateOrgViewBag();
            return View(userDTO);
        }

        public IActionResult AccessDenied() 
        {
            return View();
        }
        public async Task<IActionResult> Logout() 
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.Session.Remove(SD.SessionToken);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> RegisterAdmin()
        {
            await PopulateOrgViewBag();
            return View(new UserCreateDTO
            {
                Email = string.Empty,
                Emri = string.Empty,
                Passwordi = string.Empty,
            });
        }
        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAdmin(UserCreateDTO userDTO)
        {
            try
            {
                userDTO.NjesiaId = null;
                ApiResponse<UserReadDTO>? response = await _authService.RegisterAdminAsync<ApiResponse<UserReadDTO>>(userDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Regjistrimi u be me sukses!";
                    await PopulateOrgViewBag();
                    return View();
                }
                else
                {

                    TempData["error"] = response?.Message ?? "Regjistrimi deshtoj. Ju lutem provoni perseri.";
                    await PopulateOrgViewBag();
                    return View(userDTO);
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            await PopulateOrgViewBag();
            return View(userDTO);
        }

        private async Task PopulateNjesiteViewBag()
        {
            var response = await _njesiaService.GetByOrgAsync<ApiResponse<List<NjesiReadDto>>>();
            var njesia = response?.Data;
            ViewBag.NjesiteList = new SelectList(njesia, "NjesiteId", "Emri");
        }

        [HttpGet]
        public async Task<IActionResult> RegisterManager()
        {
            await PopulateNjesiteViewBag();
            return View(new UserCreateDTO
            {
                Email = string.Empty,
                Emri = string.Empty,
                Passwordi = string.Empty
            });
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterManager(UserCreateDTO userDTO)
        {
            try
            {
                userDTO.BiznesId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "BiznesId")?.Value ?? "0");
                ApiResponse<UserReadDTO>? response = await _authService.RegisterManagerAsync<ApiResponse<UserReadDTO>>(userDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Regjistrimi u be me sukses!";
                    await PopulateNjesiteViewBag();
                    return View();
                }
                else
                {

                    TempData["error"] = response?.Message ?? "Regjistrimi deshtoj. Ju lutem provoni perseri.";
                    await PopulateNjesiteViewBag();
                    return View(userDTO);
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            await PopulateNjesiteViewBag();
            return View(userDTO);
        }
    }
}
