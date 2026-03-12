using AutoMapper;
using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services;
using Parking_web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Parking_web.Controllers
{
    public class SherbimiController : Controller
    {
        private readonly ISherbimiService _sherbimiService;
        private readonly INjesiaService _njesiaService;
        private readonly IMapper _mapper;

        public SherbimiController(ISherbimiService sherbimiService, IMapper mapper, INjesiaService njesiaService)
        {
            _sherbimiService = sherbimiService;
            _njesiaService = njesiaService;
            _mapper = mapper;
        }
        
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Manager"))
            {
                return RedirectToAction("Index", "Njesia");
            }
            return RedirectToAction("Index2", "Njesia");
        }

        [HttpGet]
        [Authorize(Roles = "Admin , Manager")]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin , Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SherbimiCreateDTO createDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(createDTO);
            }

            try
            {
                createDTO.BiznesId = int.Parse(User.FindFirst("BiznesId")!.Value);
                var response = await _sherbimiService.CreateAsync<ApiResponse<SherbimiCreateDTO>>(createDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Sherbimi u krijua me sukses";
                    if (User.IsInRole("Manager"))
                    {
                        return RedirectToAction("Index", "Njesia");
                    }
                    return RedirectToAction("Index2", "Njesia");
                }
                else
                {
                    TempData["error"] = $"Gabim: {response?.Message ?? "Diçka shkoi keq."}";
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View(createDTO);
        }



        [Authorize(Roles = "Admin , Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = "ID e gabuar."; 
                if (User.IsInRole("Manager"))
                {
                    return RedirectToAction("Index", "Njesia");
                }
                return RedirectToAction("Index2", "Njesia");

            }

            try
            {
                var response = await _sherbimiService.GetAsync<ApiResponse<Sherbimi>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    return View(response.Data);
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin , Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Sherbimi sherbimi)
        {
            try
            {
                var response = await _sherbimiService.DeleteAsync<ApiResponse<object>>(sherbimi.SherbimiId);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Sherbimi u fshi me sukses";
                }
                else
                {
                    TempData["error"] = "Fshirja e sherbimit deshtoi";
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            if (User.IsInRole("Manager"))
            {
                return RedirectToAction("Index", "Njesia");
            }
            return RedirectToAction("Index2", "Njesia");
        }


        [Authorize(Roles = "Admin , Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = "ID e gabuar.";
                if (User.IsInRole("Manager"))
                {
                    return RedirectToAction("Index", "Njesia");
                }
                return RedirectToAction("Index2", "Njesia");
            }

            try
            {
                var response = await _sherbimiService.GetAsync<ApiResponse<Sherbimi>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    return View(_mapper.Map<SherbimiUpdateDTO>(response.Data));
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin , Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SherbimiUpdateDTO sherbimi)
        {
            try
            {
                var response = await _sherbimiService.UpdateAsync<ApiResponse<object>>(sherbimi);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Sherbimi u permirsua me sukses";
                    if (User.IsInRole("Manager"))
                    {
                        return RedirectToAction("Index", "Njesia");
                    }
                    return RedirectToAction("Index2", "Njesia");
                }
                else
                {
                    TempData["error"] = "Konflikt ne perditsimin e sherbimit";
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View(sherbimi);
        }

        
        [Authorize(Roles = "Admin , Manager")]
        public async Task<IActionResult> CreateParking(int? id)
        {
            var njesiteResponse = await _njesiaService.GetByOrgAsync<ApiResponse<List<NjesiReadDto>>>();
            var njesite = njesiteResponse?.Data;

            ViewBag.NjesiteList = new SelectList(njesite, "NjesiteId", "Emri", id);
            ViewBag.IsFixed = id.HasValue;
            if(id != null)
            {
                return View(new SherbimParkingDTO { NjesiteId = id.Value });
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin , Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateParking(SherbimParkingDTO createDTO)
        {
            try
            {
                createDTO.BiznesId = int.Parse(User.FindFirst("BiznesId")!.Value);
                var response = await _sherbimiService.CreateParkingAsync<ApiResponse<SherbimParkingDTO>>(createDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Sherbimi u krijua me sukses";
                    if (User.IsInRole("Manager"))
                    {
                        return RedirectToAction("Index", "Njesia");
                    }
                    return RedirectToAction("Index2", "Njesia");
                }
                else
                {
                    TempData["error"] = "Konflikt ne krijimin e sherbimit";
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View(createDTO);
        }


    }
}
