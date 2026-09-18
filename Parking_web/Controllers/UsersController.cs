using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;

namespace Parking_web.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IUserOrgService _userOrgService;

        public UsersController(IUserService userService,IUserOrgService userOrgService)
        {
            _userService = userService;
            _userOrgService = userOrgService;
        }

        [Authorize(Roles = "Admin , Super Admin")]
        public async Task<IActionResult> Index(string? search, string? role, int page = 1, int pageSize = 10)
        {
            bool active = false;

            var biznesIdClaim = User.FindFirst("BiznesId")?.Value;
            if (string.IsNullOrEmpty(biznesIdClaim) || biznesIdClaim == "0")
            {
                TempData["error"] = "Zgjedh një organizatë";
                return RedirectToAction("Index", "Organizata");
            }
            int biznesId = int.Parse(biznesIdClaim);
            
            var response = await _userService.GetUsersPaginationAsync<ApiResponse<UserPage>>(biznesId, search, role, active, page, pageSize);
            if (response == null || !response.Success || response.Data == null)
            {
                ViewBag.Error = response?.Message;
                return View(await _userService.GetUsersPaginationAsync<ApiResponse<UserPage>>(biznesId, null, null, active, page, pageSize));
            }

            ViewBag.Search = search;
            ViewBag.Role = role;

            return View(response.Data);
        }

        [HttpPost]
        [Authorize(Roles = "Admin , Super Admin")]
        public async Task<IActionResult> RemoveUserFromOrg(int userId)
        {
            try
            {
                var response = await _userOrgService.DeleteUserOrgAsync<ApiResponse<object>>(userId);

                if (response != null && response.Success)
                {
                    TempData["success"] = "Përdoruesi u largua me sukses";
                }
                else
                {
                    TempData["error"] = "Gabim gjat largimit të përdoruesit";
                }
            }
            catch
            {
                TempData["error"] = "Gabim gjat largimit të përdoruesit";
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin , Super Admin")]
        public async Task<IActionResult> Details(int id)
        {
            UserTotalsDTO rez = new UserTotalsDTO();
            int biznesId = int.Parse(User.FindFirst("BiznesId")?.Value ?? "0");

            var response = await _userService.GetTotalsAsync<ApiResponse<UserTotalsDTO>>(id, biznesId);
            if (response != null && response.Success && response.Data != null)
            {
                rez = response.Data;
            }
            
            return View(rez);
        }
    }
}
