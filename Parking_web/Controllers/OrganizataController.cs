using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;
using System.Drawing.Printing;
using System.Security.Claims;

namespace Parking_web.Controllers
{
    public class OrganizataController : Controller
    {
        private readonly IOrganizataService _organizataService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public OrganizataController(IOrganizataService organizataService, IUserService userService, IMapper mapper)
        {
            _organizataService = organizataService;
            _userService = userService;
            _mapper = mapper;
        }

        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> Index(string? Search, int page = 1, int pageSize = 10)
        {
            OrgPage orgList = new();
            try
            {
                var response = await _organizataService.GetPaginationAsync<ApiResponse<OrgPage>>(Search, null, false, false, page, pageSize);
                if (response != null && response.Success && response.Data != null)
                {
                    orgList = response.Data;
                }

                string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdStr))
                {
                    int userId = int.Parse(userIdStr);
                    var userResponse = await _userService.GetAsync<ApiResponse<Useri>>(userId);
                    if (userResponse != null && userResponse.Success && userResponse.Data != null)
                    {
                        ViewBag.OrgId = userResponse.Data.UserOrgs.FirstOrDefault()?.BiznesId;
                    }
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View(orgList);
        }


        [Authorize(Roles= "Super Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new OrgCreateDTO
            {
                AllowCustomers = true
            };

            return View(model);
        }
        
        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrgCreateDTO createDTO)
        {
            try
            {
                createDTO.Admin?.Email = createDTO.Email;
                var response = await _organizataService.CreateAsync<ApiResponse<Organizata>>(createDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Organizata u krijua me sukses";
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



        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if(id<= 0)
            {
                TempData["error"] = "ID e gabuar.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var response = await _organizataService.GetAsync<ApiResponse<Organizata>>(id);
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
        [Authorize(Roles = "Super Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Organizata org)
        {
            try
            {
                var response = await _organizataService.DeleteAsync<ApiResponse<object>>(org.BiznesId);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Organizata u fshi me sukses";
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
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                TempData["error"] = "ID e gabuar.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var response = await _organizataService.GetAsync<ApiResponse<Organizata>>(id);
                if (response != null && response.Success && response.Data != null)
                {
                    return View(_mapper.Map<OrgUpdateDTO>(response.Data));
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
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrgUpdateDTO org)
        {
            try
            {
                var response = await _organizataService.UpdateAsync<ApiResponse<object>>(org);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Organizata u permirsua me sukses";
                }
                else
                {
                    TempData["error"] = $"Gabim: {response?.Message ?? "Diçka shkoi keq."}";
                    return View(org);
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int orgId, bool activate)
        {
            try
            {
                var response = await _userService.ActivateSuperAdminAsync<ApiResponse<string>>(orgId);
                if (response != null && response.Success)
                {
                    var orgResponse = await _organizataService.GetAsync<ApiResponse<Organizata>>(orgId);
                    if (orgResponse != null && orgResponse.Success && orgResponse.Data != null)
                    {
                        if (activate)
                        {
                            await AuthController.RefreshUserClaims(HttpContext, orgId.ToString(), orgResponse.Data.EmriBiznesit);
                        }
                        else
                        {
                            await AuthController.RefreshUserClaims(HttpContext, null, null);
                        }

                        if (!string.IsNullOrEmpty(response.Data))
                        {
                            HttpContext.Session.SetString(SD.SessionToken, response.Data);
                        }
                        TempData["success"] = "Llogaria u permirsua me sukses";
                    }
                    else
                    {
                        TempData["error"] = $"Gabim: {response?.Message ?? "Nuk u gjet Organizata."}";
                    }
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
            return RedirectToAction(nameof(Index));
        }
    }
}
