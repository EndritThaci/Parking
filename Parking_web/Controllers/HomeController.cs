using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Parking_web.Helpers;
using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services.IServices;
using QRCoder;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Xml.Linq;

namespace Parking_web.Controllers
{
    public class HomeController : Controller
    {
        private readonly INjesiaService _njesiaService;
        private readonly IOrganizataService _orgService;
        private readonly ITransaksionService _transaksioniService;
        private readonly ICilsimiService _cilsimiService;
        private readonly ISherbimiService _sherbimiService;
        private readonly ICreditCardService _creditCardService;
        private readonly IMapper _mapper;

        public HomeController(IOrganizataService organizataService,INjesiaService njesiaService, ITransaksionService transaksioniService, ISherbimiService sherbimiService, ICilsimiService cilsimiService, IMapper mapper, ICreditCardService creditCardService)
        {
            _orgService = organizataService;
            _njesiaService = njesiaService;
            _transaksioniService = transaksioniService;
            _cilsimiService = cilsimiService;
            _sherbimiService = sherbimiService;
            _mapper = mapper;
            _creditCardService = creditCardService;
        }

        public async Task<IActionResult> Index(string? Search, int page = 1, int pageSize = 9)
        {
            OrgPage orgPage = new();
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (page < 1) page = 1;
                var response = await _orgService.GetPaginationAsync<ApiResponse<OrgPage>>(Search, userId, page, pageSize);
                var pendingResponse = await _transaksioniService.GetPendingAsync<ApiResponse<List<TransaksionRead>>>(userId);

                if (response != null && response.Success && response.Data != null)
                {
                    orgPage = response.Data;
                }
                if (pendingResponse != null && pendingResponse.Success && pendingResponse.Data != null && User.IsInRole("Customer"))
                {
                    ViewBag.PendingTransactions = pendingResponse.Data;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View(orgPage);
        }

        [HttpGet]
        public async Task<IActionResult> GetNjesite(int orgId)
        {
            try
            {
                if (orgId <= 0) return BadRequest();

                var response = await _njesiaService.GetByOrgAsync<ApiResponse<List<NjesiOrg>>>( orgId);

                if (response == null || !response.Success || response.Data == null)
                {
                    return Json(new { success = false, message = response?.Message ?? "Nuk u gjetën njësitë." });
                }
                var njesiaId = 0;
                if (response.Data.Count == 1) njesiaId = response.Data[0].NjesiteId;

                return Json(new { success = true, data = response.Data, njesiaId = njesiaId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Create(int njesiaId)
        {
            TransaksionetCreateDto createDto = new();
            try
            {
                var njesiaResponse = await _njesiaService.GetAsync<ApiResponse<NjesiOrg>>(njesiaId);
                if (njesiaResponse == null || njesiaResponse.Data == null)
                {
                    TempData["error"] = "Nuk u gjet njesi";
                    return RedirectToAction("Index");
                }

                if (njesiaResponse.Data.VendeTeLira <= 0)
                {
                    TempData["error"] = $"Nuk ka vende të lira në njësinë {njesiaResponse.Data.Emri}";
                    return RedirectToAction("Index");
                }

                var cilsimiResponse = await _cilsimiService.GetByNjesiAsync<ApiResponse<List<CilsimetReadDto>>>(njesiaId);
                var cilsimet = cilsimiResponse?.Data;
                var cilsimiActiv = cilsimet?.FirstOrDefault(c => c.Selected);

                if (cilsimiActiv == null)
                {
                    TempData["error"] = "Nuk u gjet asnje cilesim aktiv për kete njësi.";
                    return RedirectToAction("Index");
                }

                ViewBag.NjesiaEmri = njesiaResponse.Data.Emri;
                ViewBag.QRScanner = njesiaResponse.Data.QRScanner;
                ViewBag.CilsimiEmri = cilsimiActiv.Emri;

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
        public async Task<IActionResult> EntryQR(int njesiaId, string? identifikues)
        {
            ViewBag.UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            ViewBag.Identifikues = identifikues;
            return View(njesiaId);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> QRReader(string encrypted, int? njesiaId, string? identifikues, int? id, int? cardId, string? type)
        {
            QREncrypt? qrData = Encryption.DecryptQR(encrypted);
            if (qrData == null)
            {
                return new JsonResult(new { success = false, message = "Ky QR Kod është i pavlefshëm." });
            }

            if (qrData.Type == "Edit" && qrData.ID != null)
            {
                var expectedSignature = GenerateSignature(id: qrData.ID);
                if (qrData.Signature != expectedSignature)
                {
                    return new JsonResult(new { success = false, message = "Ky QR Kod është i pavlefshëm." });
                }

                return new JsonResult(new { success = true, message = "Sherbimet e Transaksionit", redirectUrl = Url.Action("Edit", "Home", new { transaksioniId = qrData.ID }) });
            }
            else if (qrData.Type == "Payment")
            {
                if (qrData.ID == null || qrData.CardID == null)
                {
                    return new JsonResult(new { success = false, message = "Ky QR Kod është i pavlefshëm." });
                }

                string expectedSignature = GenerateSignature(id: qrData.ID, timestamp: qrData.Timestamp, cardId: qrData.CardID);
                if (qrData.Signature != expectedSignature)
                {
                    return new JsonResult(new { success = false, message = "Ky QR Kod është i pavlefshëm." });
                }

                if (DateTime.TryParseExact(qrData.Timestamp, "yyyyMMddHHmm", null, System.Globalization.DateTimeStyles.None, out DateTime generatedTime))
                {
                    var diff = DateTime.UtcNow - generatedTime;

                    if (diff.TotalMinutes > 10)
                    {
                        return new JsonResult( new { success = false, message = "Ky QR Kod ka skaduar (limiti 10 min). Ju lutem gjeneroni një të ri." });
                    }

                    var res = await PayTransactionFunction(qrData.ID.Value, qrData.CardID.Value, null, null);
                    return new JsonResult(new { success = res.Success, message = res.Message });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Format i gabuar i kohës" });
                }
            }
            else if (qrData.Type == "Entry")
            {
                if (qrData.NjesiaID == null || qrData.UserID == null)
                {
                    return new JsonResult(new { success = false, message = "Ky QR Kod është i pavlefshëm." });
                }
                if (qrData.NjesiaID != njesiaId)
                {
                    return new JsonResult(new { success = false, message = "Ky QR Kod është për njësi tjetër." });
                }

                string expectedSignature = GenerateSignature(njesiaId: qrData.NjesiaID, userId: qrData.UserID);
                if (qrData.Signature != expectedSignature)
                {
                    return new JsonResult(new { success = false, message = "Ky QR Kod është i pavlefshëm." });
                }

                var res = await CreateTransactionFunction(qrData.NjesiaID.Value, qrData.UserID.Value, qrData.Identifikues);
                return new JsonResult(new { success = res.Success, message = res.Message });
            }
            else if (qrData.Type == "Njesia" && type == "Exit")
            {
                if (qrData.NjesiaID == null )
                {
                    return new JsonResult(new { success = false, message = "Ky QR Kod është i pavlefshëm." });
                }
                else if (id == null || cardId == null)
                {
                    return new JsonResult(new { success = false, message = "Gabim gjat përpunimit të të dhënave" });
                }

                string expectedSignature = GenerateSignature(njesiaId: qrData.NjesiaID);
                if (qrData.Signature != expectedSignature)
                {
                    return new JsonResult(new { success = false, message = "Ky QR Kod është i pavlefshëm." });
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var res = await PayTransactionFunction(id.Value, cardId.Value, qrData.NjesiaID.Value, userId);
                return new JsonResult(new { success = res.Success, message = res.Message });
            }
            else if (qrData.Type == "Njesia")
            {
                if (qrData.NjesiaID == null)
                {
                    return new JsonResult(new { success = false, message = "Ky QR Kod është i pavlefshëm." });
                }

                string expectedSignature = GenerateSignature(njesiaId: qrData.NjesiaID);
                if (qrData.Signature != expectedSignature)
                {
                    return new JsonResult(new { success = false, message = "Ky QR Kod është i pavlefshëm." });
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var res = await CreateTransactionFunction(qrData.NjesiaID.Value, userId, identifikues);
                return new JsonResult(new { success = res.Success, message = res.Message });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Ky QR Kod është i pavlefshëm." });
            }
        }

        private async Task<(bool Success, string? Message)> CreateTransactionFunction(int njesiaId, int userId, string? identifikues)
        {
            TransaksionetCreateDto createDto = new();
            try
            {
                var cilsimiResponse = await _cilsimiService.GetByNjesiAsync<ApiResponse<List<CilsimetReadDto>>>(njesiaId);
                var cilsimet = cilsimiResponse?.Data;
                var cilsimiActiv = cilsimet?.FirstOrDefault(c => c.Selected);

                if (cilsimiActiv == null)
                {
                    return ( false, "Nuk u gjet asnjë cilësim aktiv për këtë njësi." );
                }

                createDto.NjesiaId = njesiaId;
                createDto.CilsimiId = cilsimiActiv.CilsimetiId;
                createDto.UserId = userId;
                createDto.Identifikues = identifikues;

                var response = await _transaksioniService.CreateAsync<ApiResponse<TransaksionetCreateDto>>(createDto);
                if (response != null && response.Success && response.Data != null)
                {
                    return ( true, "Transaksioni u krijua me sukses." );

                }
                return ( false, "Ndodhi një gabim gjatë krijimit të transaksionit." );

            }
            catch (Exception ex)
            {
                return ( false, $"Gabim: {ex.Message}" );
            }
        }
        private async Task<(bool Success, string? Message)> PayTransactionFunction(int transactionId, int cardId, int? njesiaId, int? userId)
        {
            try
            {
                var transResponse = await _transaksioniService.GetAsync<ApiResponse<TransaksionRead>>(transactionId);
                if (transResponse == null || !transResponse.Success || transResponse.Data == null)
                {
                    return (false, "Transaksioni nuk u gjet." );
                }
                if (transResponse.Data.Statusi != "Pending")
                {
                    return (false, "Transaksioni është paguar." );
                }
                if (userId != null && userId != transResponse.Data.Useri.UserId)
                {
                    return (false, " Ky transaksioni nuk është i juaji.");
                }

                decimal amount = transResponse.Data.Cmimi ?? 0;
                if (amount < 0)
                {
                    return (false, "Shuma e transaksionit është e pavlefshme." );
                }

                if (!njesiaId.HasValue)
                {
                    if (!int.TryParse(User.FindFirst("NjesiaId")?.Value, out int userNjesiaId))
                    {
                        return (false, "Nuk u gjet njësia e përdoruesit." );
                    }

                    njesiaId = userNjesiaId;
                }
                if (transResponse.Data.Njesia.NjesiteId != njesiaId)
                {
                    return (false, "Ky QR Kod është për njësi tjetër." );
                }

                var dto = new PayRequestDto();
                dto.CreditCardId = cardId;
                dto.Amount = amount;

                var response = await _creditCardService.PayAsync<ApiResponse<CreditCardReadDto>>(dto);
                if (response != null && response.Success)
                {
                    var responsePay = await _transaksioniService.PayAsync<ApiResponse<TransaksionRead>>(transactionId);
                    if (responsePay != null && responsePay.Success)
                    {
                        return (true, "Pagesa u krye me sukses!" );
                    }
                    return (false, "Pagesa u regjistrua si sukses por dështoi në marrjen e parave");
                }
                else
                {
                    return (false, $"{response?.Message ?? "Pagesa dështoi"}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Gabim: {ex.Message}");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTransacsion(int njesiaId, int userId, string? identifikues)
        {
            var result = await CreateTransactionFunction( njesiaId, userId, identifikues );

            TempData[result.Success ? "success" : "error"] = result.Message;
            return RedirectToAction( User.IsInRole("Customer") ? "Index" : "Employee" );
        }

        [HttpGet]
        [Authorize(Roles = "Employee , Manager , Admin")]
        public async Task<IActionResult> Edit(int transaksioniId)
        {
            if (transaksioniId <= 0)
            {
                TempData["error"] = "ID e gabuar.";
                return RedirectToAction("Employee");
            }

            try
            {
                var response = await _transaksioniService.GetAsync<ApiResponse<TransaksionRead>>(transaksioniId);
                var sherbimiResponse = await _sherbimiService.GetByOrgAsync<ApiResponse<List<Sherbimi>>>();
                if (response != null && response.Success && response.Data != null)
                {
                    var transaction = response.Data;
                    if (User.IsInRole("Employee") || User.IsInRole("Manager"))
                    {
                        if (!int.TryParse( User.FindFirst("NjesiaId")?.Value, out int njesiaId))
                        {
                            TempData["error"] = "Nuk u gjet njësia e përdoruesit.";
                            return RedirectToAction("Employee");
                        }
                        if (transaction.Njesia == null || njesiaId != transaction.Njesia.NjesiteId)
                        {
                            TempData["error"] = "Ky transaksion nuk ndodhet në këtë njësi.";
                            return RedirectToAction("Employee");
                        }
                    }
                    else if (User.IsInRole("Admin"))
                    {
                        if (!int.TryParse( User.FindFirst("BiznesId")?.Value, out int orgId))
                        {
                            TempData["error"] = "Nuk u gjet organizata e përdoruesit.";
                            return RedirectToAction("Employee");
                        }
                        if (transaction.Njesia == null || orgId != transaction.Njesia.BiznesId)
                        {
                            TempData["error"] = "Ky transaksion nuk ndodhet në këtë organizatë.";
                            return RedirectToAction("Employee");
                        }
                    }

                    ViewBag.Sherbimet = sherbimiResponse?.Data;

                    ViewBag.ExistingSherbim = response.Data.Sherbimi?.Where(s => s.Cmimi != 0).Select(s => s.SherbimiId).ToList() ?? new List<int>();
                    return View(response.Data);
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return RedirectToAction("Employee");
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
            var response = await _transaksioniService.GetPriceAsync<ApiResponse<TransaksionRead>>(id);

            if (response == null || !response.Success)
            {
                TempData["error"] = $"Gabim: {response?.Message ?? "Transaksioni nuk u gjet."}";
                if (User.IsInRole("Customer"))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Employee");
                }
            }
            var cardDetails = await _creditCardService.GetByUserAsync<ApiResponse<IEnumerable<CreditCardReadDto>>>();
            if (cardDetails != null && cardDetails.Success)
            {
                ViewBag.CardDetails = cardDetails.Data;
            }

            return View(response.Data);
        }

        [HttpPost]
        [Authorize(Roles = "Employee , Manager , Admin")]
        public async Task<IActionResult> CashPayment(int id)
        {
            try
            {
                var transaksioni = await _transaksioniService.GetAsync<ApiResponse<TransaksionRead>>(id);
                if (transaksioni == null || !transaksioni.Success || transaksioni.Data == null)
                {
                    TempData["error"] = $"Gabim: {transaksioni?.Message ?? "Diçka shkoi keq."}";
                    return RedirectToAction("Pay", new {id});
                }

                var response = await _transaksioniService.PayAsync<ApiResponse<TransaksionRead>>(id);
                if (response != null && response.Success)
                {
                    TempData["success"] = "Transaksioni u mbyll me sukses. Faleminderit për përdorimin e Parkingut tonë";
                    if (User.IsInRole("Customer"))
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("Employee");
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
            return RedirectToAction("Pay", new { id });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> QRShow(int id, int selectedCardId)
        {
            var response = await _creditCardService.GetAsync<ApiResponse<CreditCardReadDto>>(selectedCardId);
            if (response != null && response.Success && response.Data != null)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (response.Data.UserId == userId)
                {
                    ViewBag.SelectedCardId = selectedCardId;
                    return View(id);
                }
                else
                {
                    TempData["error"] = $"Gabim: Karta nuk ekziston.";
                    return RedirectToAction("Index");
                }
            }
            TempData["error"] = $"Gabim1234: {response?.Message ?? "Karta nuk ekziston."}";
            return RedirectToAction("Index");
        }

        public IActionResult QRGenerate(int? id, int? selectedCardId, int? njesiaId, int? userId, string? identifikues, string? type)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmm");
                string encrypted = "";

                if (selectedCardId != null && id != null)
                {
                    string signature = GenerateSignature(id: id, timestamp: timestamp, cardId: selectedCardId);
                    var qrData = new QREncrypt { ID = id, Timestamp = timestamp, Signature = signature, CardID = selectedCardId, Type = "Payment" };
                    encrypted = Encryption.EncryptQR(qrData);
                }
                else if (njesiaId != null && userId != null)
                {
                    string signature = GenerateSignature(njesiaId: njesiaId, userId: userId);
                    var qrData = new QREncrypt { NjesiaID = njesiaId, UserID = userId , Signature = signature, Identifikues = identifikues, Type = "Entry"};
                    encrypted = Encryption.EncryptQR(qrData);
                }
                else if (njesiaId != null)
                {
                    string signature = GenerateSignature(njesiaId: njesiaId);
                    var qrData = new QREncrypt { NjesiaID = njesiaId, Signature = signature, Type = "Njesia"};
                    encrypted = Encryption.EncryptQR(qrData);
                }
                else if (id != null)
                {
                    string signature = GenerateSignature(id: id);
                    var qrData = new QREncrypt { ID = id, Signature = signature, Type = "Edit" };
                    encrypted = Encryption.EncryptQR(qrData);
                }
                else
                {
                    TempData["error"] = "Gabim. QR kodi nuk mund te krijohet ";
                    return View("Index");
                }

                QRCodeData qrCodeData = qrGenerator.CreateQrCode(encrypted, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);

                return File(qrCodeAsPngByteArr, "image/png");
            }
        }

        private string GenerateSignature(int? id = null, string? timestamp = null, int? cardId = null, int? njesiaId = null, int? userId = null)
        {
            string secretKey = "hfxycvrdsxr653eed6>";
            string payload = "";
            if (id != null && timestamp != null && cardId != null)
            {
                payload = $"{id}-{timestamp}-{cardId}";
            }
            else if (njesiaId != null && userId != null)
            {
                payload = $"{njesiaId}-{userId}";
            }

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
                TempData["error"] = "Ky QR Kod nuk eksiston.";
                return RedirectToAction("Index");
            }

            string expectedSignature = GenerateSignature(id: id, timestamp: t, cardId: c);
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
                TempData["error"] = "FORMAT I GABUAR I KOHES";
                return RedirectToAction("Index");
            }

            return View(id);
        }

        [HttpGet]
        [Authorize(Roles = "Employee , Manager , Admin , Super Admin")]
        public async Task<IActionResult> Employee(int? njesia)
        {
            List<TransaksionRead>? pending = new List<TransaksionRead>();
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                ViewBag.UserId = userId;
                if((User.IsInRole("Admin") || User.IsInRole("Super Admin")) && njesia == null)
                {
                    if (int.TryParse(User.FindFirst("BiznesId")?.Value, out int orgId) && orgId > 0)
                    {
                        var response = await _njesiaService.GetByOrgAsync<ApiResponse<List<NjesiOrg>>>(orgId);
                        if (response != null && response.Success && response.Data != null && response.Data.Count == 1)
                        {
                            return RedirectToAction("Employee", "Home", new { njesia = response.Data[0].NjesiteId });
                        }
                    }
                    return RedirectToAction("SelectNjesi");
                }

                if (!int.TryParse(User.FindFirst("NjesiaId")?.Value, out int njesiaId) || njesiaId == 0)
                {
                    if (njesia != null)
                    {
                        njesiaId = njesia.Value;
                    }
                    else
                    {
                        throw new Exception("Nuk e keni Njesinë të konfiguruar");
                    }
                }
                ViewBag.NjesiaId = njesiaId;
                var njesiaResponse = await _njesiaService.GetAsync<ApiResponse<NjesiOrg>>(njesiaId);
                if (njesiaResponse != null && njesiaResponse.Success && njesiaResponse.Data != null)
                {
                    ViewBag.VendeTeLira = njesiaResponse.Data.VendeTeLira;
                }

                var pendingResponse = await _transaksioniService.GetPendingAsync<ApiResponse<List<TransaksionRead>>>(null, njesiaId);

                if (pendingResponse != null && pendingResponse.Success)
                {
                    pending = pendingResponse.Data;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View(pending);
        }

        [HttpGet]
        [Authorize(Roles = "Admin , Super Admin")]
        public async Task<IActionResult> SelectNjesi()
        {
            var njesite = new List<NjesiOrg>();
            try
            {
                if ((!int.TryParse(User.FindFirst("BiznesId")?.Value, out int orgId) || orgId <= 0) && User.IsInRole("Super Admin"))
                {
                    TempData["error"] = "Zgjedh një organizatë";
                    return RedirectToAction("Index", "Organizata");
                }

                var response = await _njesiaService.GetByOrgAsync<ApiResponse<List<NjesiOrg>>>(orgId);
                if (response == null || !response.Success || response.Data == null)
                {
                    TempData["error"] = "Nuk u gjetën njësitë.";
                }
                else if (response.Data.Count == 1)
                {
                    return RedirectToAction("Employee", "Home", new {njesia = response.Data[0].NjesiteId});
                }
                else
                {
                    njesite = response.Data;
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Gabim: {ex.Message}";
            }
            return View(njesite);
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