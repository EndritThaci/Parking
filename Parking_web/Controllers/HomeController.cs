using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;
using QRCoder;
using System.Diagnostics;

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
        private readonly ICardDetailsService _cardDetailsService;
        private readonly IMapper _mapper;

        public HomeController(INjesiaService njesiaService, ILokacioniService lokacioniService, ITransaksionService transaksioniService, IVendiService vendiService, ISherbimiService sherbimiService, ICilsimiService cilsimiService, IMapper mapper, ICardDetailsService cardDetailsService)
        {
            _njesiaService = njesiaService;
            _lokacioniService = lokacioniService;
            _vendiService = vendiService;
            _transaksioniService = transaksioniService;
            _cilsimiService = cilsimiService;
            _sherbimiService = sherbimiService;
            _mapper = mapper;
            _cardDetailsService = cardDetailsService;
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
            catch (Exception ex)
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
                var cilsimiActiv = cilsimet?.FirstOrDefault(c => c.Selected);

                if (cilsimiActiv == null)
                {
                    TempData["error"] = "Nuk u gjet asnje cilesim aktiv për kete njësi.";
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EntryQR(int njesiaId, int vendiId)
        {
            ViewBag.VendiId = vendiId;
            return View(njesiaId);
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EntryQRReader(int njesiaId, int vendiId)
        {
            ViewBag.VendiId = vendiId;
            return View(njesiaId);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTransacsion(int njesiaId, int vendiId)
        {
            TransaksionetCreateDto createDto = new();
            try
            {
                var cilsimiResponse = await _cilsimiService.GetByNjesiAsync<ApiResponse<List<CilsimetReadDto>>>(njesiaId);
                var cilsimet = cilsimiResponse.Data;
                var cilsimiActiv = cilsimet?.FirstOrDefault(c => c.Selected);

                if (cilsimiActiv == null)
                {
                    TempData["error"] = "Nuk u gjet asnje cilesim aktiv për kete njësi.";
                    return RedirectToAction("Index");
                }
                createDto.VendiParkimitId = vendiId;
                createDto.NjesiaId = njesiaId;
                createDto.CilsimiId = cilsimiActiv.CilsimetiId;
 
                var response = await _transaksioniService.CreateAsync<ApiResponse<TransaksionetCreateDto>>(createDto);
                if (response != null && response.Success && response.Data != null)
                {
                    TempData["success"] = "Transaksioni u krijua me sukses";
                    return RedirectToAction("Index");
                }
                TempData["error"] = $"Gabim: Ndodhi nje gabim gjat krijimit te transaksionit";

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return RedirectToAction("Index");
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
                var response = await _transaksioniService.UpdateAsync<ApiResponse<TransaksionRead>>(transaksioniId, transaksion);
                if (response != null && response.Success)
                {
                    return RedirectToAction("Pay", new { id = transaksioniId });
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
            return RedirectToAction("Edit", new { transaksioniId = transaksioniId });
        }

        [HttpGet]
        public async Task<IActionResult> Pay(int id)
        {
            var response = await _transaksioniService.GetAsync<ApiResponse<TransaksionRead>>(id);

            if (response == null || !response.Success)
            {
                TempData["error"] = $"Gabim: {response?.Message ?? "Transaksioni nuk u gjet."}";
                return RedirectToAction("Index");
            }
            var cardDetails = await _cardDetailsService.GetByUserAsync<ApiResponse<IEnumerable<CardDetails>>>();
            if (cardDetails != null && cardDetails.Success)
            {
                ViewBag.CardDetails = cardDetails.Data;
            }

            return View(response.Data);
        }

        [HttpPost]
        [Authorize(Roles = "Manager , Admin")]
        public async Task<IActionResult> CashPayment(int id)
        {
            try
            {
                var transaksioni = await _transaksioniService.GetAsync<ApiResponse<TransaksionRead>>(id);
                if (transaksioni == null || !transaksioni.Success || transaksioni.Data == null)
                {
                    TempData["error"] = $"Gabim: {transaksioni?.Message ?? "Diçka shkoi keq."}";
                    return RedirectToAction("Index");
                }

                var response = await _transaksioniService.PayAsync<ApiResponse<TransaksionRead>>(id);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Transaksioni u mbyll me sukses. Faleminderit për përdorimin e Parkingut tonë";
                    return RedirectToAction("Index");
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
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public IActionResult QRShow(int id, int selectedCardId)
        {
            ViewBag.SelectedCardId = selectedCardId;
            return View(id);
        }

        public IActionResult QRGenerate(int? id, int? selectedCardId, int? njesiaId, int? vendiId)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmm");
                string url = "";

                if (selectedCardId != null && id != null)
                {
                    string signature = GenerateSignature((int)id, timestamp, (int)selectedCardId);
                    url = $"https://localhost:7061/Home/QRRead?id={id}&t={timestamp}&c={selectedCardId}&s={signature}"; // URL dohet me bo te serverit
                }
                else if (njesiaId != null && vendiId != null)
                {
                    url = $"https://localhost:7061/Home/EntryQRReader?njesiaId={njesiaId}&vendiId={vendiId}"; // URL dohet me bo te serverit
                }
                else
                {
                    TempData["error"] = "Gabim. QR kodi nuk mund te krijohet ";
                    return View("Index");
                }
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);

                return File(qrCodeAsPngByteArr, "image/png");
            }
        }

        private string GenerateSignature(int id, string timestamp, int cardId)
        {
            string secretKey = "hfxycvrdsxr653eed6>";
            string payload = $"{id}-{timestamp}-{cardId}";

            using (var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
                return Convert.ToBase64String(hashBytes).Replace("+", "-").Replace("/", "_");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> QRRead(int id, string t, int c, string s)
        {
            if (string.IsNullOrEmpty(t) || string.IsNullOrEmpty(s))
            {
                TempData["error"] = "Ky QR Kod nuk exsiston.";
                return RedirectToAction("Index");
            }

            string expectedSignature = GenerateSignature(id, t, c);
            if (s != expectedSignature)
            {
                TempData["error"] = "Ky QR Kod është i pavlefshëm.";
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
                ViewBag.SelectedCardId = c;

            }
            else
            {
                return BadRequest("Format i gabuar i kohës.");
            }

            return View(id);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> QRWrite(int id, int CardId)
        {
            try
            {

                var transaksioni = await _transaksioniService.GetAsync<ApiResponse<TransaksionRead>>(id);
                if (transaksioni == null || !transaksioni.Success || transaksioni.Data == null)
                {
                    TempData["error"] = "QR Code i pavlefshëm.";
                    return RedirectToAction("Index");
                }

                var card = await _cardDetailsService.PayAsync<ApiResponse<CardDetails>>(CardId, transaksioni.Data.Cmimi!.Value);
                if (card == null || !card.Success)
                {
                    TempData["error"] = $"Pagesa deshtoi. {card?.Message ?? ""}";
                    return RedirectToAction("Index");
                }

                //TransaksionUpdateDto update = new();
                //update.SherbimiId = transaksioni.Data.Sherbimi?.Where(i=> i.SherbimiId != transaksioni.Data.Cilsimi.SherbimiId).Select(i=> i.SherbimiId).ToList();
                //var response1 = await _transaksioniService.UpdateAsync<ApiResponse<TransaksionRead>>(id, update);
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