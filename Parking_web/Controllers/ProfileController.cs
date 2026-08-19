using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;
using System.Security.Claims;

namespace Parking_web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly INjesiaService _njesiaService;
        private readonly ISherbimiService _shebimiService;
        private readonly ICilsimiService _cilsimiService;
        private readonly IDetajetService _detajetService;
        private readonly IUserService _userService; 
        private readonly ICreditCardService _creditCardService;
        private readonly IMapper _mapper;

        public ProfileController(INjesiaService njesiaService, IMapper mapper, ISherbimiService shebimiService, ICilsimiService cilsimiService, IDetajetService detajetService, IUserService userService, ICreditCardService creditCardService)
        {
            _njesiaService = njesiaService;
            _mapper = mapper;
            _shebimiService = shebimiService;
            _cilsimiService = cilsimiService;
            _detajetService = detajetService;
            _userService = userService;
            _creditCardService = creditCardService;
        }


        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Auth");

            int userId = int.Parse(userIdClaim);

            var user = await _userService.GetAsync<ApiResponse<Useri>>(userId);
            if (user == null) return NotFound();

            var cardDetails = await _creditCardService.GetByUserAsync<ApiResponse<IEnumerable<CreditCardReadDto>>>();
            if (cardDetails != null && cardDetails.Success)
            {
                ViewBag.CardDetails = cardDetails.Data;
            }

            return View(user.Data);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO dto)
        {
            try
            {
                var response = await _userService.ChangePasswordAsync<ApiResponse<Useri>>(dto);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Passwordi u përmirësua me sukses";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = response.Message ?? "U shfaq një gabim gjat përmirësimit të passwordit";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "U shfaq një gabim gjat përmirësimit të passwordit: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangeName(UserUpdateDTO dto)
        {
            try
            {
                dto.UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                dto.BiznesId = int.Parse(User.FindFirst("BiznesId")?.Value ?? "0");
                var response = await _userService.UpdateAsync<ApiResponse<Useri>>(dto);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Të dhënat u përmirësuan me sukses";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = response?.Message ?? "U shfaq një gabim gjat përmirësimit të të dhënave";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "U shfaq një gabim gjat përmirësimit të të dhënave: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}
