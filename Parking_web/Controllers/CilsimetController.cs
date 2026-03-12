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
    public class CilsimetController : Controller
    {
        private readonly ICilsimiService _cilsimiService;
        private readonly INjesiaService _njesiaService;
        private readonly ISherbimiService _sherbimiService;
        private readonly IMapper _mapper;

        public CilsimetController(ICilsimiService cilsimiService,INjesiaService njesiaService, IMapper mapper, ISherbimiService sherbimiService)
        {
            _cilsimiService = cilsimiService;
            _njesiaService = njesiaService;
            _mapper = mapper;
            _sherbimiService = sherbimiService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Manager"))
            {
                return RedirectToAction("Index", "Njesia");
            }
            return RedirectToAction("Index2", "Njesia");
        }

        private async Task PopulateViewBag(int? id)
        {
            var njesiteResponse = await _njesiaService.GetByOrgAsync<ApiResponse<List<NjesiReadDto>>>();
            var sherbimiResponse = await _sherbimiService.GetByOrgAsync<ApiResponse<List<Sherbimi>>>();
            var njesite = njesiteResponse?.Data;
            var sherbimet = sherbimiResponse?.Data;

            var sherbimetMeId = sherbimet?.Select(n => new {
                SherbimiId = n.SherbimiId,
                EmriMeId = $"{n.Emri} (ID: {n.SherbimiId})"
            }).ToList();

            ViewBag.NjesiteList = new SelectList(njesite, "NjesiteId", "Emri", id);
            ViewBag.SherbimetList = new SelectList(sherbimetMeId, "SherbimiId", "EmriMeId");
            ViewBag.IsFixed = id.HasValue;
        }

        [HttpGet]
        [Authorize(Roles = "Admin , Manager")]
        public async Task<IActionResult> Create(int? id)
        {
            await PopulateViewBag(id);
            if (id != null)
            {
                return View(new CilsimetReadDto {NjesiteId = id.Value});
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin , Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CilsimetCreateDto createDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(_mapper.Map<CilsimetReadDto>(createDTO));
            }

            try
            {
                var response = await _cilsimiService.CreateAsync<ApiResponse<CilsimetCreateDto>>(createDTO);
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
            if (User.IsInRole("Manager"))
            {
                await PopulateViewBag(createDTO.NjesiteId);
            }
            else
            {
                await PopulateViewBag(null);
            }
            return View(_mapper.Map<CilsimetReadDto>(createDTO));
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
                var response = await _cilsimiService.GetAsync<ApiResponse<CilsimetReadDto>>(id);
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
        public async Task<IActionResult> Delete(CilsimetReadDto cilsimi)
        {
            try
            {
                var response = await _cilsimiService.DeleteAsync<ApiResponse<object>>(cilsimi.CilsimetiId);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Sherbimi u fshi me sukses";
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
        public async Task<IActionResult> Edit(int id, int? njesiaId)
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
                await PopulateViewBag(njesiaId);

                var response = await _cilsimiService.GetAsync<ApiResponse<CilsimetReadDto>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    return View(_mapper.Map<CilsimetUpdateDto>(response.Data));
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
        public async Task<IActionResult> Edit(CilsimetUpdateDto cilsimi)
        {
            try
            {
                var response = await _cilsimiService.UpdateAsync<ApiResponse<object>>(cilsimi);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Cilsimi u permirsua me sukses";

                }
                else
                {
                    TempData["error"] = $"Gabim: {response?.Message ?? "Diçka shkoi keq."}";
                    if (User.IsInRole("Manager"))
                    {
                        await PopulateViewBag(cilsimi.NjesiteId);
                    }
                    else
                    {
                        await PopulateViewBag(null);
                    }
                    return View(cilsimi);
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

        [HttpPost]
        [Authorize(Roles = "Admin , Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                var response = await _cilsimiService.ActivateAsync<ApiResponse<object>>(id);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Cilsimi u aktivizua me sukses.";
                }
                else
                {
                    TempData["error"] = "Aktivizimi deshtoi.";
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
