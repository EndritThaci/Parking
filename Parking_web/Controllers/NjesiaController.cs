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
        private readonly IOrganizataService _orgService;
        private readonly ISherbimiService _shebimiService;
        private readonly ICilsimiService _cilsimiService;
        private readonly IDetajetService _detajetService;
        private readonly IMapper _mapper;

        public NjesiaController(INjesiaService njesiaService, IMapper mapper, IOrganizataService organizataService, ISherbimiService shebimiService, ICilsimiService cilsimiService, IDetajetService detajetService)
        {
            _njesiaService = njesiaService;
            _orgService = organizataService;
            _mapper = mapper;
            _shebimiService = shebimiService;
            _cilsimiService = cilsimiService;
            _detajetService = detajetService;
        }
        

        [Authorize(Roles = "Manager, Admin , Super Admin")]
        public async Task<IActionResult> Index()
        {
            if (User.FindFirst("BiznesId")?.Value == "0")
            {
                TempData["error"] = "Zgjedh një organizatë";
                if(User.IsInRole("Admin")) return RedirectToAction("Index", "Home");
                return RedirectToAction("Index", "Organizata");
            }

            List<NjesiReadDto> orgList = new();

            if (User.IsInRole("Manager")){
                try
                {
                    int njesiaId = int.Parse(User.FindFirst("NjesiaId")!.Value);
                    ViewBag.NjesiaId = njesiaId;
                    var response = await _njesiaService.GetAsync<ApiResponse<NjesiReadDto>>(njesiaId);
                    var sherbimiResponse = await _shebimiService.GetByOrgAsync<ApiResponse<List<Sherbimi>>>();
                    var cilsimiResponse = await _cilsimiService.GetByNjesiAsync<ApiResponse<List<CilsimetReadDto>>>(njesiaId);
                    var detajetResponse = await _detajetService.GetByNjesiAsync<ApiResponse<List<DetajetReadDto>>>();
                    if (response != null && response.Success && response.Data != null)
                    {
                        orgList.Add(response.Data);
                        ViewBag.customers = response.Data.Organizata.AllowCustomers;
                        ViewBag.Sherbimet = sherbimiResponse?.Data;
                        ViewBag.Cilsimet = cilsimiResponse?.Data;
                        ViewBag.Detajet = detajetResponse?.Data;
                    }
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Gabim: {ex.Message}";
                }
            }
            else
            {
                try
                {
                    var response = await _njesiaService.GetByOrgAsync<ApiResponse<List<NjesiReadDto>>>();
                    var sherbimiResponse = await _shebimiService.GetByOrgAsync<ApiResponse<List<Sherbimi>>>();
                    var cilsimiResponse = await _cilsimiService.GetByOrgAsync<ApiResponse<List<CilsimetReadDto>>>();
                    var detajetResponse = await _detajetService.GetByOrgAsync<ApiResponse<List<DetajetReadDto>>>();
                    if (response != null && response.Success && response.Data != null)
                    {
                        orgList = response.Data;
                        ViewBag.customers = response.Data[0].Organizata.AllowCustomers;
                        ViewBag.Sherbimet = sherbimiResponse?.Data;
                        ViewBag.Cilsimet = cilsimiResponse?.Data;
                        ViewBag.Detajet = detajetResponse?.Data;
                    }
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Gabim: {ex.Message}";
                }
            }
            return View(orgList);
        }

        [Authorize(Roles = "Admin , Super Admin")]
        public async Task<IActionResult> Create()
        {
            var orgId = int.Parse(User.FindFirst("BiznesId")!.Value);
            var response = await _orgService.GetAsync<ApiResponse<Organizata>>(orgId);
            if (response != null && response.Success && response.Data != null)
            {
                ViewBag.customers = response.Data.AllowCustomers;
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin , Super Admin")]
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
                    return RedirectToAction(nameof(Index));
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



        [Authorize(Roles = "Admin , Super Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = "ID e gabuar.";
                return RedirectToAction(nameof(Index));
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
        [Authorize(Roles = "Admin , Super Admin")]
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
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin , Super Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = "ID e gabuar.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var response = await _njesiaService.GetAsync<ApiResponse<NjesiReadDto>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    ViewBag.customers = response.Data.Organizata.AllowCustomers;
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
        [Authorize(Roles = "Admin , Super Admin")]
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
                    return RedirectToAction(nameof(Index));
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

        [Authorize(Roles = "Manager, Admin , Super Admin")]
        public async Task<IActionResult> NjesiaQR(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = "ID e gabuar.";
                return RedirectToAction(nameof(Index));
            }
            if (User.IsInRole("Manager"))
            {
                if (!int.TryParse(User.FindFirst("NjesiaId")?.Value, out int njesiaId))
                {
                    TempData["error"] = "Nuk u gjet njësia e përdoruesit.";
                    return RedirectToAction("Index");
                }

                id = njesiaId;
            }
            else
            {
                var response = await _njesiaService.GetAsync<ApiResponse<NjesiReadDto>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    if(int.Parse(User.FindFirst("BiznesId")!.Value) != response.Data.BiznesId)
                    {
                        TempData["error"] = "Nuk keni qasje në këtë njësi.";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData["error"] = "Kjo njësi nuk ekziston.";
                    return RedirectToAction("Index");
                }
            }
            return View(id);
        }
    }
}
