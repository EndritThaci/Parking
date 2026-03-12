using AutoMapper;
using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services;
using Parking_web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Parking_web.Controllers
{
    public class LokacioniController : Controller
    {

        private readonly ILokacioniService _lokacioniService;
        private readonly INjesiaService _njesiaService;
        private readonly IMapper _mapper;

        public LokacioniController(ILokacioniService lokacioniService,INjesiaService njesiaService ,IMapper mapper)
        {
            _lokacioniService = lokacioniService;
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

        private async Task populateViewBag(int? id)
        {
            var njesiteResponse = await _njesiaService.GetByOrgAsync<ApiResponse<List<NjesiReadDto>>>();
            var njesite = njesiteResponse?.Data;

            ViewBag.NjesiteList = new SelectList(njesite, "NjesiteId", "Emri", id);
            ViewBag.IsFixed = id.HasValue;

        }

        [HttpGet]
        [Authorize(Roles = "Admin , Manager")]
        public async Task<IActionResult> Create(int? id)
        {
            await populateViewBag(id);
            if (id != null)
            {
                return View(new Lokacioni { NjesiteId = id.Value });
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin , Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LokacioniCreateDTO lokacioni)
        {
            if (!ModelState.IsValid)
            {
                if(User.IsInRole("Manager"))
                {
                    await populateViewBag(lokacioni.NjesiteId);
                }
                else
                {
                    await populateViewBag(null);
                }
                return View(lokacioni);
            }

            try
            {
                var response = await _lokacioniService.CreateAsync<ApiResponse<CilsimetCreateDto>>(lokacioni);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Lokacioni u krijua me sukses";
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
                await populateViewBag(lokacioni.NjesiteId);
            }
            else
            {
                await populateViewBag(null);
            }
            return View(_mapper.Map<Lokacioni>(lokacioni));
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
                var response = await _lokacioniService.GetAsync<ApiResponse<Lokacioni>>(id);
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
        public async Task<IActionResult> Delete(Lokacioni lokacioni)
        {
            try
            {
                var response = await _lokacioniService.DeleteAsync<ApiResponse<object>>(lokacioni.LokacioniId);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Lokacioni u fshi me sukses";
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
                await populateViewBag(njesiaId);

                var response = await _lokacioniService.GetAsync<ApiResponse<Lokacioni>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    return View(_mapper.Map<Lokacioni>(response.Data));
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
        public async Task<IActionResult> Edit(LokacioniUpdateDTO lokacioni)
        {
            try
            {
                var response = await _lokacioniService.UpdateAsync<ApiResponse<object>>(lokacioni);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Lokacioni u permirsua me sukses";
                }
                else
                {
                    TempData["error"] = $"Gabim: {response?.Message ?? "Diçka shkoi keq."}";
                    if (User.IsInRole("Manager"))
                    {
                        await populateViewBag(lokacioni.NjesiteId);
                    }
                    else
                    {
                        await populateViewBag(null);
                    }
                    return View(_mapper.Map<Lokacioni>(lokacioni));
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
