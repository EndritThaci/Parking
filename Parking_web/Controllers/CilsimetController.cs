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
            return RedirectToAction("Index", "Njesia");
        }

        private async Task PopulateViewBag(int? id)
        {
            var njesiteResponse = await _njesiaService.GetByOrgAsync<ApiResponse<List<NjesiReadDto>>>();
            var sherbimiResponse = await _sherbimiService.GetByOrgAsync<ApiResponse<List<Sherbimi>>>();
            var njesite = njesiteResponse?.Data;
            var sherbimet = sherbimiResponse?.Data;

            var sherbimetMeId = sherbimet?.Where(s => s.Cmimi == 0).Select(n => new {
                SherbimiId = n.SherbimiId,
                EmriMeId = $"{n.Emri} (ID: {n.SherbimiId})"
            }).ToList();

            ViewBag.NjesiteList = new SelectList(njesite, "NjesiteId", "Emri", id);
            ViewBag.SherbimetList = new SelectList(sherbimetMeId, "SherbimiId", "EmriMeId");
            ViewBag.IsFixed = id.HasValue;
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin, Admin , Manager")]
        public async Task<IActionResult> Create()
        {
            int? id = User.IsInRole("Manager") ? int.Parse(User.FindFirst("NjesiaId")!.Value) : null;
            await PopulateViewBag(id);
            if (id != null)
            {
                return View(new CilsimetWithDetailsCreateDTO { NjesiteId = id.Value});
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin , Admin , Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CilsimetWithDetailsCreateDTO createDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(createDTO);
            }

            try
            {
                var response = await _cilsimiService.CreateWithDetailAsync<ApiResponse<CilsimetReadDto>>(createDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Cilesimi u krijua me sukses";
                    return RedirectToAction("Index", "Njesia");
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
            return View(createDTO);
        }



        [Authorize(Roles = "Super Admin , Admin , Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = "ID e gabuar.";
                return RedirectToAction("Index", "Njesia");

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
        [Authorize(Roles = "Super Admin , Admin , Manager")]
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
            return RedirectToAction("Index", "Njesia");
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin , Admin , Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = "ID e gabuar.";
                return RedirectToAction("Index", "Njesia");
            }

            try
            {
                int? njesiaId = User.IsInRole("Manager") ? int.Parse(User.FindFirst("NjesiaId")!.Value) : null;
                await PopulateViewBag(njesiaId);

                var response = await _cilsimiService.GetAsync<ApiResponse<CilsimetReadDto>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    return View(_mapper.Map<CilsimetWithDetailsUpdateDTO>(response.Data));
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin , Admin , Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CilsimetWithDetailsUpdateDTO cilsimi)
        {
            try
            {
                var response = await _cilsimiService.UpdateWithDetailsAsync<ApiResponse<object>>(cilsimi);
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
            return RedirectToAction("Index", "Njesia");
        }
        
        [HttpGet]
        [Authorize(Roles = "Super Admin , Admin , Manager")]
        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = "ID e gabuar.";
                return RedirectToAction("Index", "Njesia");
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
            TempData["error"] = "Ka ndodhur një gabim";
            return RedirectToAction("Index", "Njesia");
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin , Admin , Manager")]
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
            return RedirectToAction("Index", "Njesia");
        }
    }
}
