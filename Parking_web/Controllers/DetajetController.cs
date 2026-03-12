using AutoMapper;
using Parking_web.Models.DTO;
using Parking_web.Services;
using Parking_web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace Parking_web.Controllers
{
    public class DetajetController : Controller
    {
        private readonly IDetajetService _detajetService;
        private readonly ICilsimiService _cilsimiService;
        private readonly IMapper _mapper;

        public DetajetController(IDetajetService detajetService, IMapper mapper, ICilsimiService cilsimiService)
        {
            _detajetService = detajetService;
            _mapper = mapper;
            _cilsimiService = cilsimiService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Manager"))
            {
                return RedirectToAction("Index", "Njesia"); ;
            }
            return RedirectToAction("Index2", "Njesia"); ;
        }

        private async Task populateViewBag()
        {
            List<CilsimetReadDto>? cilsimiet = new List<CilsimetReadDto>();
            if (User.IsInRole("Manager"))
            {
                var cilsimiResponse = await _cilsimiService.GetByNjesiAsync<ApiResponse<List<CilsimetReadDto>>>(int.Parse(User.FindFirst("NjesiaId")?.Value ?? ""));
                cilsimiet = cilsimiResponse?.Data;
            }
            else
            {
                var cilsimiResponse = await _cilsimiService.GetByOrgAsync<ApiResponse<List<CilsimetReadDto>>>();
                cilsimiet = cilsimiResponse?.Data;
            }


            ViewBag.CilsimiList = new SelectList(cilsimiet, "CilsimetiId", "Emri");

        }

        [HttpGet]
        [Authorize(Roles = "Admin , Manager")]
        public async Task<IActionResult> Create()
        {
            await populateViewBag();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin , Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DetajetCreateDto createDTO)
        {
            if (!ModelState.IsValid)
            {
                await populateViewBag();
                return View(createDTO);
            }

            try
            {
                var response = await _detajetService.CreateAsync<ApiResponse<DetajetCreateDto>>(createDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Detaji u krijua me sukses";
                    if (User.IsInRole("Manager"))
                    {
                        return RedirectToAction("Index", "Njesia");
                    }
                    return RedirectToAction("Index2", "Njesia");
                }
                TempData["error"] = $"Gabim: {response?.Message ?? "Diçka shkoi keq."}";

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            await populateViewBag();
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
                var response = await _detajetService.GetAsync<ApiResponse<DetajetReadDto>>(id);
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
        public async Task<IActionResult> Delete(DetajetReadDto detaji)
        {
            try
            {
                var response = await _detajetService.DeleteAsync<ApiResponse<object>>(detaji.DetajetId);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Detaji u fshi me sukses";
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
            if (User.IsInRole("Manager"))
            {
                return RedirectToAction("Index", "Njesia");
            }
            return RedirectToAction("Index2", "Njesia");
        }

        [HttpGet]
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
                var response = await _detajetService.GetAsync<ApiResponse<DetajetReadDto>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    return View(_mapper.Map<DetajetUpdateDto>(response.Data));
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
        public async Task<IActionResult> Edit(int id,DetajetUpdateDto detajet)
        {
            try
            {
                var response = await _detajetService.UpdateAsync<ApiResponse<object>>(id,detajet);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Detaji u permirsua me sukses";
                }
                else
                {
                    TempData["error"] = $"Gabim: {response?.Message ?? "Diçka shkoi keq."}";
                    return View(detajet);
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
    }
}
