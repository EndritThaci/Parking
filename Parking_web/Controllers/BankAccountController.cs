using AutoMapper;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Parking_web.Controllers
{
    public class BankAccountController : Controller
    {
        private readonly ICreditCardService _creditCardService;
        private readonly IMapper _mapper;

        public BankAccountController(IMapper mapper, ICreditCardService creditCardService)
        {
            _mapper = mapper;
            _creditCardService = creditCardService;
        }

        private async Task populateViewBag()
        {
            var cardDetails = await _creditCardService.GetByUserAsync<ApiResponse<IEnumerable<CreditCardReadDto>>>();
            if (cardDetails != null && cardDetails.Success)
            {
                ViewBag.CardDetails = cardDetails.Data;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> deleteCard(int id)
        {
            var result = await _creditCardService.DeleteAsync<ApiResponse<CreditCardReadDto>>(id);
            if (result == null || !result.Success)
            {
                TempData["error"] = result?.Message ?? "U shfaq një gabim";
            }
            await populateViewBag();

            return RedirectToAction("index", "Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> createCard(string paymentMethodId)
        {
            if (string.IsNullOrEmpty(paymentMethodId))
            {
                TempData["error"] = "Kartela nuk u krijua.";
                return RedirectToAction("index", "Profile");
            }

            var dto = new CreditCardCreateDto
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
                StripePaymentMethodId = paymentMethodId
            };

            var result = await _creditCardService.CreateAsync<ApiResponse<CreditCardReadDto>>(dto);

            if (result == null || !result.Success)
            {
                TempData["error"] = result?.Message ?? "Gabim gjatë ruajtjes së kartelës";
            }

            await populateViewBag();

            TempData["success"] = "Kartela u shtua me sukses.";
            return RedirectToAction("index", "Profile");
        }
    }
}