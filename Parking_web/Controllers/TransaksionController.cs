using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services;
using Parking_web.Services.IServices;
using System.Drawing.Printing;

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

        [Authorize(Roles = "Admin , Super Admin")]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, int njesia = -1)
        {
            if (User.FindFirst("BiznesId")?.Value == "")
            {
                TempData["error"] = "Zgjedh një organizatë";
                return RedirectToAction("Index", "Organizata");
            }

            try
            {
                var response = await _transaksionService.GetAsync<ApiResponse<TransaksionPage>>(pageNumber, pageSize, njesia);
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

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> IndexManager(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var response = await _transaksionService.GetAsync<ApiResponse<TransaksionPage>>(pageNumber, pageSize, 0);
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
    }
}
