using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;

namespace Parking_web.Controllers
{
    public class BankAccountController : Controller
    {
        private readonly ICardDetailsService _cardDetailsService;
        private readonly IBankService _bankService;
        private readonly IMapper _mapper;

        public BankAccountController(ICardDetailsService cardDetailsService, IMapper mapper, IBankService bankService)
        {
            _cardDetailsService = cardDetailsService;
            _mapper = mapper;
            _bankService = bankService;
        }

        private async Task populateViewBag()
        {
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
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> createAccount(CardAcountCreateDTO dto)
        {
            var result = await _cardDetailsService.CreateAccountAsync<ApiResponse<CardDetails>>(dto);
            if (result == null || !result.Success || result.Data == null)
            {
                TempData["error"] = result?.Message ?? "U shfaq një gabim";
            }

            await populateViewBag();

            return RedirectToAction("index", "Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> deleteAccount(int id)
        {
            var result = await _cardDetailsService.DeleteAsync<ApiResponse<CardDetails>>(id);
            if (result == null || !result.Success || result.Data == null)
            {
                TempData["error"] = result?.Message ?? "U shfaq një gabim";
            }
            await populateViewBag();

            return RedirectToAction("index", "Profile");
        }
    }
}