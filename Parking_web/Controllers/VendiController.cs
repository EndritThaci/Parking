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
    public class VendiController : Controller
    {

        private readonly IVendiService _vendiService;
        private readonly ILokacioniService _lokacioniService;
        private readonly IMapper _mapper;

        public VendiController(IVendiService vendiService, IMapper mapper, ILokacioniService lokacioniService)
        {
            _vendiService = vendiService;
            _lokacioniService = lokacioniService;
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

        private async Task populateViewBag()
        {
            List<Lokacioni>? lokacionet = new List<Lokacioni>();

            if (User.IsInRole("Manager"))
            {
                var lokacioniResponse = await _lokacioniService.GetByNjesiAsync<ApiResponse<List<Lokacioni>>>(int.Parse(User.FindFirst("NjesiaId")?.Value ?? ""));
                lokacionet = lokacioniResponse?.Data;
            }
            else
            {
                var lokacioniResponse = await _lokacioniService.GetByOrgAsync<ApiResponse<List<Lokacioni>>>();
                lokacionet = lokacioniResponse?.Data;
            }

            var lokacionKati = lokacionet?.Select(k => new
            {
                LokacioniId = k.LokacioniId,
                KatiNjesi = $"Kati {k.Kati} ne {k.NjesiOrg.Emri}"
            }).ToList();

            ViewBag.LokacioniList = new SelectList(lokacionKati, "LokacioniId", "KatiNjesi");

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
        public async Task<IActionResult> Create(VendiCreateDTO vendi)
        {
            if (!ModelState.IsValid)
            {
                await populateViewBag();
                return View(vendi);
            }

            try
            {
                var response = await _vendiService.CreateAsync<ApiResponse<VendiCreateDTO>>(vendi);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Vendi u krijua me sukses";
                    if (User.IsInRole("Manager"))
                    {
                        return RedirectToAction("Index", "Njesia");
                    }
                    return RedirectToAction("Index2", "Njesia");
                }
                else
                {
                    TempData["error"] = $"Gabim: {response?.Message ?? "Ndodhi nje gabim"}";
                }
            

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            await populateViewBag();
            return View(_mapper.Map<Vendi>(vendi));
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
                var response = await _vendiService.GetAsync<ApiResponse<Vendi>>(id);
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
        public async Task<IActionResult> Delete(Vendi vendi)
        {
            try
            {
                var response = await _vendiService.DeleteAsync<ApiResponse<object>>(vendi.VendiId);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Vendi u fshi me sukses";
                }
                else
                {
                    TempData["error"] = $"Gabim: {response?.Message ?? "Ndodhi nje gabim"}";
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
                await populateViewBag();

                var response = await _vendiService.GetAsync<ApiResponse<Vendi>>(id);
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
        public async Task<IActionResult> Edit(VendiUpdateDTO vendi)
        {
            try
            {
                var response = await _vendiService.UpdateAsync<ApiResponse<object>>(vendi);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Vendi u permirsua me sukses";
                }
                else
                {
                    TempData["error"] = $"Gabim: {response?.Message ?? "Ndodhi nje gabim"}";
                    await populateViewBag();
                    return View(_mapper.Map<Vendi>(vendi));
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
