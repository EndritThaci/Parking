using AutoMapper;
using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Parking_web.Controllers
{
    public class OrganizataController : Controller
    {
        private readonly IOrganizataService _organizataService;
        private readonly IMapper _mapper;

        public OrganizataController(IOrganizataService organizataService, IMapper mapper)
        {
            _organizataService = organizataService;
            _mapper = mapper;
        }

        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> Index()
        {
            List<Organizata> orgList = new();
            try
            {
                var response = await _organizataService.GetAllAsync<ApiResponse<List<Organizata>>>();
                if (response != null && response.Success && response.Data != null)
                {
                    orgList = response.Data;
                }

            }catch(Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View(orgList);
        }


        [Authorize(Roles= "Super Admin")]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        
        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrgCreateDTO createDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(createDTO);
            }

            try
            {
                var response = await _organizataService.CreateAsync<ApiResponse<Organizata>>(createDTO);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Organizata u krijua me sukses";
                    return RedirectToAction(nameof(Index));
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

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
