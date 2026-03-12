using AutoMapper;
using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Parking_web.Controllers
{
    public class NjesiaController : Controller
    {
        private readonly INjesiaService _njesiaService;
        private readonly ISherbimiService _shebimiService;
        private readonly ICilsimiService _cilsimiService;
        private readonly IDetajetService _detajetService;
        private readonly ILokacioniService _lokacioniService;
        private readonly IVendiService _vendiService;
        private readonly IMapper _mapper;

        public NjesiaController(INjesiaService njesiaService, IMapper mapper, ISherbimiService shebimiService, ICilsimiService cilsimiService, IDetajetService detajetService, ILokacioniService lokacioniService, IVendiService vendiService)
        {
            _njesiaService = njesiaService;
            _mapper = mapper;
            _shebimiService = shebimiService;
            _cilsimiService = cilsimiService;
            _detajetService = detajetService;
            _lokacioniService = lokacioniService;
            _vendiService = vendiService;
        }
        

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index2()
        {
            List<NjesiReadDto> orgList = new();
            try
            {
                var response = await _njesiaService.GetByOrgAsync<ApiResponse<List<NjesiReadDto>>>();
                var sherbimiResponse = await _shebimiService.GetByOrgAsync<ApiResponse<List<Sherbimi>>>();
                var cilsimiResponse = await _cilsimiService.GetByOrgAsync<ApiResponse<List<CilsimetReadDto>>>();
                var detajetResponse = await _detajetService.GetByOrgAsync<ApiResponse<List<DetajetReadDto>>>();
                var lokacionetResponse = await _lokacioniService.GetByOrgAsync<ApiResponse<List<Lokacioni>>>();
                var vendiResponse = await _vendiService.GetByOrgAsync<ApiResponse<List<Vendi>>>();
                if (response != null && response.Success && response.Data != null)
                {
                    orgList = response.Data;
                    ViewBag.Sherbimet = sherbimiResponse?.Data;
                    ViewBag.Cilsimet = cilsimiResponse?.Data;
                    ViewBag.Detajet = detajetResponse?.Data;
                    ViewBag.Lokacionet = lokacionetResponse?.Data;
                    ViewBag.Vendet = vendiResponse?.Data;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View(orgList);
        }

        //[Authorize(Roles = "Customer")]
        public async Task<IActionResult> Index()
        {
            NjesiReadDto orgList = new();
            try
            {
                int njeisaId = int.Parse(User.FindFirst("NjesiaId")!.Value);
                var response = await _njesiaService.GetAsync<ApiResponse<NjesiReadDto>>(njeisaId);
                var sherbimiResponse = await _shebimiService.GetByOrgAsync<ApiResponse<List<Sherbimi>>>();
                var cilsimiResponse = await _cilsimiService.GetByNjesiAsync<ApiResponse<List<CilsimetReadDto>>>(njeisaId);
                var detajetResponse = await _detajetService.GetByNjesiAsync<ApiResponse<List<DetajetReadDto>>>();
                var lokacionetResponse = await _lokacioniService.GetByNjesiAsync<ApiResponse<List<Lokacioni>>>(njeisaId);
                var vendiResponse = await _vendiService.GetByNjesiAsync<ApiResponse<List<Vendi>>>();
                if (response != null && response.Success && response.Data != null)
                {
                    orgList = response.Data;
                    ViewBag.Sherbimet = sherbimiResponse?.Data;
                    ViewBag.Cilsimet = cilsimiResponse?.Data;
                    ViewBag.Detajet = detajetResponse?.Data;
                    ViewBag.Lokacionet = lokacionetResponse?.Data;
                    ViewBag.Vendet = vendiResponse?.Data;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View(orgList);
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NjesiOrgDto createDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(createDTO);
            }

            try
            {
                createDTO.BiznesId = int.Parse(User.FindFirst("BiznesId")!.Value);
                var response = await _njesiaService.CreateAsync<ApiResponse<NjesiReadDto>>(createDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Njesia u krijua me sukses";
                    return RedirectToAction(nameof(Index2));
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



        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = "ID e gabuar.";
                return RedirectToAction(nameof(Index2));
            }

            try
            {
                var response = await _njesiaService.GetAsync<ApiResponse<NjesiReadDto>>(id);
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
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(NjesiReadDto njesia)
        {
            try
            {
                var response = await _njesiaService.DeleteAsync<ApiResponse<object>>(njesia.NjesiteId);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Njesia u fshi me sukses";
                }
                else
                {
                    TempData["error"] = "Fshirja e njesise deshtoi";
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return RedirectToAction(nameof(Index2));
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = "ID e gabuar.";
                return RedirectToAction(nameof(Index2));
            }

            try
            {
                var response = await _njesiaService.GetAsync<ApiResponse<NjesiReadDto>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    return View(_mapper.Map<NjesiUpdateDto>(response.Data));
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NjesiUpdateDto njesi)
        {
            try
            {
                njesi.BiznesId = int.Parse(User.FindFirst("BiznesId")!.Value);
                var response = await _njesiaService.UpdateAsync<ApiResponse<object>>(njesi);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Njesia u permirsua me sukses";
                    return RedirectToAction(nameof(Index2));
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
            return View(njesi);
        }
    }
}
