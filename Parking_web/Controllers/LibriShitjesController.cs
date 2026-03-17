using AutoMapper;
using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services;
using Parking_web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;

namespace Parking_web.Controllers
{
    public class LibriShitjesController : Controller
    {
        private readonly INjesiaService _njesiaService;
        private readonly ILibriService _libriService;
        private readonly IMapper _mapper;


        public LibriShitjesController(IMapper mapper, INjesiaService njesiaService, ILibriService libriService)
        {
            _mapper = mapper;
            _njesiaService = njesiaService;
            _libriService = libriService;
        }

        [Authorize]
        public async Task<IActionResult> GetLibri(int? id, int? month, int? year, bool? all, int? njesia)
        {
            LibriShitjesCreateDTO dto = new();
            dto.id = id;
            dto.month = month;
            dto.year = year;
            dto.all = all;
            dto.njesia = njesia;

            if (User.IsInRole("Manager"))
            {
                dto.njesia = int.Parse(User.FindFirst("NjesiaId")?.Value ?? "0");
            }

            if (dto.njesia != null)
            {
                var response = await _njesiaService.GetAsync<ApiResponse<NjesiReadDto>>(dto.njesia.Value);
                if (response != null && response.Success != false)
                    ViewBag.Njesia = response?.Data;
            }

            if (id == null && month == null && year == null && (all == null || all == false))
            {
                return View();
            }
            return View(dto);
        }

        [Authorize]
        public async Task<IActionResult> GetLibriCustom()
        {
            if (User.IsInRole("Manager"))
            {
                var njesiId = int.Parse(User.FindFirst("NjesiaId")?.Value ?? "");
                var response = await _njesiaService.GetAsync<ApiResponse<NjesiReadDto>>(njesiId);
                if (response != null && response.Success != false)
                    ViewBag.Njesia = response?.Data;
            }
            else
            {
                var response = await _njesiaService.GetByOrgAsync<ApiResponse<List<NjesiReadDto>>>();
                if (response != null && response.Success != false)
                    ViewBag.Njesia = response?.Data;
            }
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GetLibri(LibriShitjesCreateDTO dto)
        {
            var response = await _libriService.GetLibriShitjesAsync<ApiResponse<byte[]>>(dto);

            if (response == null || response.Data == null || response.Success == false)
            {
                TempData["error"] = $"Gabim: {response?.Message ?? "Diçka shkoi keq."}";

                string refererUrl = Request.Headers["Referer"].ToString();

                if (!string.IsNullOrEmpty(refererUrl))
                {
                    return Redirect(refererUrl);
                }

                return RedirectToAction("GetLibriCustom");
            }
            if (dto.month != null)
            {
                var monthName = CultureInfo
                    .GetCultureInfo("sq-AL")
                    .DateTimeFormat
                    .GetMonthName(dto.month.Value);

                monthName = char.ToUpper(monthName[0]) + monthName[1..].ToLower();

                return File(
                    response.Data,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Libri_i_Shitjeve_{monthName}.xlsx");
            }
            else if (dto.year != null)
            {
                return File(
                    response.Data,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Libri_i_Shitjeve_{dto.year}.xlsx");
            }
            else if (dto.id != null)
            {
                return File(
                    response.Data,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Transaksioni_{dto.id}.xlsx");
            }
            else if (dto.all != null && dto.all.Value)
            {
                return File(
                    response.Data,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Transaksioni_all.xlsx");
            }
            TempData["error"] = $"Gabim: {response?.Message ?? "Diçka shkoi keq."}";
            return RedirectToAction("GetLibriCustom");
        }
    }
}