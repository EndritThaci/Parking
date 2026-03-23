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
        private readonly ILokacioniService _lokacioniService;
        private readonly IVendiService _vendiService;
        private readonly IUserService _userService;
        private readonly ICardDetailsService _cardDetailsService;
        private readonly IBankService _bankService;
        private readonly IMapper _mapper;

        public ProfileController(INjesiaService njesiaService, IMapper mapper, ISherbimiService shebimiService, ICilsimiService cilsimiService, IDetajetService detajetService, ILokacioniService lokacioniService, IVendiService vendiService, IUserService userService, ICardDetailsService cardDetailsService, IBankService bankService)
        {
            _njesiaService = njesiaService;
            _mapper = mapper;
            _shebimiService = shebimiService;
            _cilsimiService = cilsimiService;
            _detajetService = detajetService;
            _lokacioniService = lokacioniService;
            _vendiService = vendiService;
            _userService = userService;
            _cardDetailsService = cardDetailsService;
            _bankService = bankService;
        }


        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Auth");

            int userId = int.Parse(userIdClaim);

            var user = await _userService.GetAsync<ApiResponse<Useri>>(userId);
            if (user == null) return NotFound();

            var cardDetails = await _cardDetailsService.GetByUserAsync<ApiResponse<IEnumerable<CardDetails>>>();
            if (cardDetails != null && cardDetails.Success)
            {
                ViewBag.CardDetails = cardDetails.Data;
            }

            var banks = await _bankService.GetAllAsync<ApiResponse<IEnumerable<Banka>>>();
            if (banks != null && banks.Success)
            {
                ViewBag.Banks = banks.Data!.Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Name
                }).ToList();
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
                    TempData["success"] = "Passwordi u ndërrua me sukses";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = response.Message ?? "U shfaq një gabim gjat ndërrimit të passwordit";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "U shfaq një gabim gjat ndërrimit të passwordit: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}
