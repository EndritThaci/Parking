using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;

namespace Parking_web.Controllers
{
    public class TransaksionController : Controller
    {
        private readonly ITransaksionService _transaksionService;
        private readonly IMapper _mapper;

        public TransaksionController(ITransaksionService transaksionService, IMapper mapper)
        {
            _transaksionService = transaksionService;
            _mapper = mapper;
        }

        [Authorize(Roles = "Customer, Manager, Admin , Super Admin")]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, int njesia = -1)
        {
            if (User.IsInRole("Super Admin") && User.FindFirst("BiznesId")?.Value == "0")
            {
                TempData["error"] = "Zgjedh një organizatë";
                return RedirectToAction("Index", "Organizata");
            }

            try
            {
                if (User.IsInRole("Manager")) njesia = 0;

                var response = User.IsInRole("Customer") ?
                    await _transaksionService.GetByUserAsync<ApiResponse<TransaksionPage>>(pageNumber, pageSize, njesia) :
                    await _transaksionService.GetAsync<ApiResponse<TransaksionPage>>(pageNumber, pageSize, njesia);

                if (response != null && response.Success && response.Data != null)
                {
                    foreach (var t in response.Data.Data)
                    {
                        if (t.Statusi == "Pending")
                        {
                            t.Cmimi = null;
                            t.KohaDaljes = null;
                            t.Sherbimi = null;
                        }
                    }
                    return View(response.Data);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }

            return View(new TransaksionPage
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = 0,
                TotalRecords = 0,
                TotalAmount = 0,
                MonthlyAmount = 0,
                YearlyAmount = 0,
                Njesite = new List<NjesiOrg>(),
                Data = new List<TransaksionRead>()
            });
        }

        [HttpPost]
        [Authorize(Roles = "Manager, Admin , Super Admin")]
        public async Task<IActionResult> Delete(int id, int pageNumber = 1, int pageSize = 10, int njesia = -1)
        {
            if (njesia == 0) njesia = -1;
            try
            {
                var response = await _transaksionService.DeleteAsync<ApiResponse<object>>(id);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Transaksioni u largua me sukses.";
                    return RedirectToAction("Index", new { pageNumber, pageSize, njesia });
                }
                TempData["error"] = "ndodhur nje gabim";
            }
            catch
            {
                TempData["error"] = "ndodhur nje gabim";
            }
            return RedirectToAction("Index", new { pageNumber, pageSize, njesia });
        }
    }
}
