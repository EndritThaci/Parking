using AutoMapper;
using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services;
using Parking_web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Parking_web.Controllers
{
    public class TransaksionController : Controller
    {
        private readonly ITransaksionService _transaksionService;
        private readonly IMapper _mapper;

        public TransaksionController(ITransaksionService transaksionService, IMapper mapper)
        {
            _transaksionService = transaksionService;
            _mapper = mapper;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {

            List<TransaksionRead> orgList = new();
            try
            {
                var response = await _transaksionService.GetByOrgAsync<ApiResponse<IEnumerable<TransaksionRead>>>();
                if (response != null && response.Success && response.Data != null)
                {
                  orgList = response.Data.ToList();
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View(orgList);
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> IndexManager()
        {

            List<TransaksionRead> orgList = new();
            try
            {
                var response = await _transaksionService.GetByNjesiAsync<ApiResponse<IEnumerable<TransaksionRead>>>();
                if (response != null && response.Success && response.Data != null)
                {
                    orgList = response.Data.ToList();
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View(orgList);
        }

        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Create()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(TransaksionetCreateDto createDTO)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(createDTO);
        //    }

        //    try
        //    {
        //        var response = await _transaksionService.CreateAsync<ApiResponse<TransaksionetCreateDto>>(createDTO);
        //        if (response != null && response.Success && response.Data != null)
        //        {
        //            TempData["success"] = "Transaksioni u krijua me sukses";
        //            return RedirectToAction("Index");
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["error"] = $"Gabim: {ex.Message}";
        //    }
        //    return View(createDTO);
        //}


        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Edit(int id)
        //{
        //    if (id <= 0)
        //    {
        //        TempData["error"] = "ID e gabuar.";
        //        return RedirectToAction("Index");
        //    }

        //    try
        //    {
        //        var response = await _transaksionService.GetAsync<ApiResponse<TransaksionRead>>(id);
        //        if (response != null && response.Success && response.Data != null)
        //        {
        //            return View(_mapper.Map<TransaksionUpdateDto>(response.Data));
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["error"] = $"Gabim: {ex.Message}";
        //    }
        //    return View();
        //}

        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id,TransaksionUpdateDto transaksion)
        //{
        //    try
        //    {
        //        var response = await _transaksionService.UpdateAsync<ApiResponse<object>>(id,transaksion);
        //        if (response != null && response.Success)
        //        {
        //            TempData["success"] = "Transaksioni u kompletua me sukses";
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["error"] = $"Gabim: {ex.Message}";
        //    }
        //    return RedirectToAction("Index");
        //}
    }
}
