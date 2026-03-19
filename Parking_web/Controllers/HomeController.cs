using AutoMapper;
using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services;
using Parking_web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using QRCoder;

namespace Parking_web.Controllers
{
    public class HomeController : Controller
    {
        private readonly INjesiaService _njesiaService;
        private readonly ILokacioniService _lokacioniService;
        private readonly IVendiService _vendiService;
        private readonly ITransaksionService _transaksioniService;
        private readonly ICilsimiService _cilsimiService;
        private readonly ISherbimiService _sherbimiService;
        private readonly IMapper _mapper;

        public HomeController(INjesiaService njesiaService, ILokacioniService lokacioniService, ITransaksionService transaksioniService, IVendiService vendiService, ISherbimiService sherbimiService, ICilsimiService cilsimiService, IMapper mapper)
        {
            _njesiaService = njesiaService;
            _lokacioniService = lokacioniService;
            _vendiService = vendiService;
            _transaksioniService = transaksioniService;
            _cilsimiService = cilsimiService;
            _sherbimiService = sherbimiService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            List<NjesiReadDto> orgList = new();
            try
            {
                var response = await _njesiaService.GetByOrgAsync<ApiResponse<List<NjesiReadDto>>>();
                var UserResponse = await _transaksioniService.GetByUserAsync<ApiResponse<List<TransaksionRead>>>();
               
                if (response != null && response.Success && response.Data != null)
                {
                    orgList = response.Data;
                }
                if (UserResponse != null && UserResponse.Success && UserResponse.Data != null)
                {
                    var pendingList = UserResponse.Data.Where(t => t.Statusi == "Pending").ToList();
                    ViewBag.PendingTransactions = pendingList;
                }

            }
            catch(Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View(orgList);
        }
        public async Task<IActionResult> Vendi(int njesiaId, int? lokacioniId)
        {
            ViewBag.SelectedNjesia = njesiaId;
            List<Vendi> vendet = new();
            try
            {
                var response = await _lokacioniService.GetByNjesiAsync<ApiResponse<List<Lokacioni>>>(njesiaId);
                if (response != null && response.Success && response.Data != null)
                {
                    ViewBag.Lokacionet = response?.Data;
                }

                if (!lokacioniId.HasValue && response.Data.Any())
                {
                    lokacioniId = response.Data.OrderBy(l => l.Kati).FirstOrDefault()?.LokacioniId;
                }

                ViewBag.SelectedLokacioni = lokacioniId;

                if (lokacioniId.HasValue)
                {
                    var response1 = await _vendiService.GetByLokacionAsync<ApiResponse<List<Vendi>>>(lokacioniId);
                    if (response1 != null && response1.Success && response1.Data != null)
                    {
                        vendet = response1.Data;
                    }
                    ViewBag.SelectedLokacioni = lokacioniId;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }

            return View(vendet);
        }

        public async Task<IActionResult> Create(int njesiaId, int vendiId)
        {
            TransaksionetCreateDto createDto = new();
            try
            {
                var vendiResponse = await _vendiService.GetAsync<ApiResponse<Vendi>>(vendiId);
                if (vendiResponse == null || vendiResponse.Data == null) return NotFound();

                Vendi vendi = vendiResponse.Data;

                if (!vendi.IsFree)
                {
                    TempData["error"] = $"Vendi {vendi.VendiEmri} është i zënë";
                    return RedirectToAction("Vendi", new { njesiaId = njesiaId, lokacioniId = vendi.LokacioniId });
                }
                var cilsimiResponse = await _cilsimiService.GetByNjesiAsync<ApiResponse<List<CilsimetReadDto>>>(njesiaId);
                var cilsimet = cilsimiResponse.Data;
                var cilsimiActiv = cilsimet?.FirstOrDefault(c => c.Active);

                if (cilsimiActiv == null)
                {
                    TempData["error"] = "Nuk u gjet asnjë cilësim aktiv për këtë njësi.";
                    return RedirectToAction("Index");
                }

                ViewBag.VendiEmri = vendi.VendiEmri;
                ViewBag.Kati = vendi.Lokacioni?.Kati;
                ViewBag.NjesiaEmri = cilsimiActiv.NjesiOrg?.Emri;
                ViewBag.CilsimiEmri = cilsimiActiv.Emri;

                createDto.VendiParkimitId = vendiId;
                createDto.NjesiaId = njesiaId;
                createDto.CilsimiId = cilsimiActiv.CilsimetiId;
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View(createDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransaksionetCreateDto createDto)
        { 
            try
            {
                var response = await _transaksioniService.CreateAsync<ApiResponse<TransaksionetCreateDto>>(createDto);
                if (response != null && response.Success && response.Data != null)
                {
                    return RedirectToAction("Index");
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View(createDto);
        }

        public async Task<IActionResult> Edit(int transaksioniId)
        {
            if (transaksioniId <= 0)
            {
                TempData["error"] = "ID e gabuar.";
                return RedirectToAction("Index");
            }

            try
            {
                var response = await _transaksioniService.GetAsync<ApiResponse<TransaksionRead>>(transaksioniId);
                var sherbimiResponse = await _sherbimiService.GetByOrgAsync<ApiResponse<List<Sherbimi>>>();
                if (response != null && response.Success && response.Data != null)
                {
                    ViewBag.Sherbimet = sherbimiResponse?.Data;
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
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int transaksioniId, TransaksionUpdateDto transaksion)
        {
            try
            {
                var response = await _transaksioniService.UpdateAsync<ApiResponse<TransaksionRead>>(transaksioniId,transaksion);
                if (response != null && response.Success)
                {
                    return RedirectToAction("Pay", new { id = transaksioniId });
                }
                else
                {
                    TempData["error"] = "Gabim ne perpunimin e sherbimeve";
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return RedirectToAction("Edit", new { transaksioniId = transaksioniId });
        }

        [HttpGet]
        public async Task<IActionResult> Pay(int id)
        {
            var response = await _transaksioniService.GetAsync<ApiResponse<TransaksionRead>>(id);

            if (response == null || !response.Success)
            {
                TempData["error"] = "Transaksioni nuk u gjet.";
                return RedirectToAction("Index");
            }

            return View(response.Data);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(int id)
        {
            var response = await _transaksioniService.PayAsync<ApiResponse<TransaksionRead>>(id);

            if (response == null || !response.Success)
            {
                TempData["error"] = "Pagesa deshtoi.";
                return RedirectToAction("Pay", new { id = id });
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public IActionResult QRShow(int id)
        {
            return View(id);
        }

        public IActionResult QRGenerate(int id)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmm");
                string url = $"https://localhost:7061/Home/QRRead?id={id}&t={timestamp}"; // URL dohet me bo te serverit

                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);

                return File(qrCodeAsPngByteArr, "image/png");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> QRRead(int id, string t)
        {
            if (string.IsNullOrEmpty(t))
            {
                TempData["error"] = "Ky QR Kod nuk exsiston.";
                return RedirectToAction("Index");
            }

            if (DateTime.TryParseExact(t, "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out DateTime generatedTime))
            {
                var diff = DateTime.UtcNow - generatedTime;

                if (diff.TotalMinutes > 10)
                {
                    TempData["error"] = "Ky QR Kod ka skaduar (limiti 10 min). Ju lutem gjeneroni një të ri.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return BadRequest("Format i gabuar i kohës.");
            }

            return View(id);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> QRWrite(int id)
        {
            try
            {
                var transaksioni = await _transaksioniService.GetAsync<ApiResponse<TransaksionRead>>(id);
                if (transaksioni == null || !transaksioni.Success || transaksioni.Data == null)
                {
                    TempData["error"] = "QR Code i pavlefshëm.";
                    return RedirectToAction("Index");
                }
                TransaksionUpdateDto update = new();
                update.SherbimiId = transaksioni.Data.Sherbimi?.Where(i => i.SherbimiId != transaksioni.Data.Cilsimi.SherbimiId).Select(i => i.SherbimiId).ToList();
                var response1 = await _transaksioniService.UpdateAsync<ApiResponse<TransaksionRead>>(id, update);
                var response = await _transaksioniService.PayAsync<ApiResponse<TransaksionRead>>(id);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Transaksioni u mbyll me sukses. Faleminderit për përdorimin e Parkingut tonë";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["error"] = "QR Code i pavlefshëm.";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return RedirectToAction("Index");
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
