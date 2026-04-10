using driving_school_management.Librarys;
using driving_school_management.Models.DTOs;
using driving_school_management.Services;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace driving_school_management.Controllers
{
    [Route("ExamPayment")]
    public class ExamPaymentController : Controller
    {
        private readonly ExamPaymentService _examPaymentService;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;
        private readonly IPayPalService _payPalService;
        private readonly IMomoService _momoService;

        public ExamPaymentController(
            ExamPaymentService examPaymentService,
            IEmailService emailService,
            IWebHostEnvironment environment,
            IConfiguration config,
            IPayPalService payPalService,
            IMomoService momoService)
        {
            _examPaymentService = examPaymentService;
            _emailService = emailService;
            _environment = environment;
            _config = config;
            _payPalService = payPalService;
            _momoService = momoService;
        }

        [HttpGet("Start")]
        public IActionResult Start(int kyThiId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            try
            {
                var model = _examPaymentService.StartExamPayment(userId.Value, kyThiId);

                if (model == null || model.PhieuList == null || !model.PhieuList.Any())
                {
                    TempData["Error"] = "Không tìm thấy dữ liệu thanh toán kỳ thi.";
                    return RedirectToAction("Index", "Exam");
                }

                ViewBag.KyThiId = kyThiId;
                return View("StartPaymentExam", model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("ConfirmRegister", "Exam", new { kyThiId });
            }
        }

        [HttpPost("ChoosePaymentMethod")]
        [ValidateAntiForgeryToken]
        public IActionResult ChoosePaymentMethod(List<int> phieuIds, string method, string noiDung)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            if (phieuIds == null || phieuIds.Count == 0)
            {
                TempData["Error"] = "Không có phiếu thanh toán để xử lý.";
                return RedirectToAction("Index", "Exam");
            }

            try
            {
                var result = _examPaymentService.ChoosePaymentMethodForMany(
                    userId.Value,
                    phieuIds,
                    method,
                    noiDung ?? string.Empty
                );

                if (result <= 0)
                {
                    TempData["Error"] = "Không cập nhật được phương thức thanh toán.";
                    return RedirectToAction("Index", "Exam");
                }

                var rawIds = string.Join(",", phieuIds.Distinct());

                if (method == "VNPAY")
                    return RedirectToAction("VnPay", new { phieuIds = rawIds });

                if (method == "PAYPAL")
                    return RedirectToAction("PayPal", new { phieuIds = rawIds });

                if (method == "MOMO")
                    return RedirectToAction("MoMo", new { phieuIds = rawIds });

                TempData["Error"] = "Phương thức thanh toán chưa được hỗ trợ.";
                return RedirectToAction("Index", "Exam");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Exam");
            }
        }

        // =========================
        // VNPAY
        // =========================
        [HttpGet("VnPay")]
        public IActionResult VnPay(string phieuIds)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var parsedPhieuIds = ParsePhieuIds(phieuIds);
            if (!parsedPhieuIds.Any())
            {
                TempData["Error"] = "Danh sách phiếu không hợp lệ.";
                return RedirectToAction("Index", "Exam");
            }

            var invoices = _examPaymentService.GetVnPayExamInfo(userId.Value, parsedPhieuIds);

            if (invoices == null || !invoices.Any())
            {
                TempData["Error"] = "Không tìm thấy phiếu thanh toán.";
                return RedirectToAction("Index", "Exam");
            }

            var hd = invoices.First();

            var amountDecimal = invoices.Sum(x => x.TongTien);
            if (amountDecimal <= 0)
            {
                TempData["Error"] = "Số tiền thanh toán không hợp lệ.";
                return RedirectToAction("Index", "Exam");
            }

            long amount = (long)amountDecimal;

            string baseUrl = _config["VnPay:BaseUrl"]!;
            string tmnCode = _config["VnPay:TmnCode"]!;
            string hashSecret = _config["VnPay:HashSecret"]!;
            string orderType = _config["VnPay:OrderType"] ?? "other";
            string locale = _config["VnPay:Locale"] ?? "vn";
            string currCode = _config["VnPay:CurrCode"] ?? "VND";

            string returnUrl = Url.Action("VnPayReturn", "ExamPayment", null, Request.Scheme)!;

            HttpContext.Session.SetString("ExamVnPayPhieuIds", string.Join(",", parsedPhieuIds));

            var vnp = new VnPayLibrary();
            vnp.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnp.AddRequestData("vnp_Command", "pay");
            vnp.AddRequestData("vnp_TmnCode", tmnCode);
            vnp.AddRequestData("vnp_Amount", (amount * 100).ToString());
            vnp.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnp.AddRequestData("vnp_CurrCode", currCode);

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString()!;
            if (string.IsNullOrWhiteSpace(ip))
                ip = "127.0.0.1";

            vnp.AddRequestData("vnp_IpAddr", ip);
            vnp.AddRequestData("vnp_Locale", locale);

            string infoRaw = $"Thanh toan ky thi #{hd.PhieuId}";

            vnp.AddRequestData("vnp_OrderInfo", RemoveVietnameseSigns(infoRaw));
            vnp.AddRequestData("vnp_OrderType", orderType);
            vnp.AddRequestData("vnp_ReturnUrl", returnUrl);

            string txnRef = $"{hd.PhieuId}-{DateTime.Now:yyyyMMddHHmmss}";
            vnp.AddRequestData("vnp_TxnRef", txnRef);

            string paymentUrl = vnp.CreateRequestUrl(baseUrl, hashSecret);
            return Redirect(paymentUrl);
        }

        [HttpGet("VnPayReturn")]
        public async Task<IActionResult> VnPayReturn()
        {
            var vnp = new VnPayLibrary();

            foreach (var key in Request.Query.Keys)
                vnp.AddResponseData(key, Request.Query[key]!);

            string secureHash = Request.Query["vnp_SecureHash"]!;
            bool isValid = vnp.ValidateSignature(secureHash, _config["VnPay:HashSecret"]!);

            if (!isValid)
            {
                ViewBag.Message = "Chữ ký VNPAY không hợp lệ.";
                return View("PaymentFail");
            }

            string txnRef = vnp.GetResponseData("vnp_TxnRef");
            int phieuId = int.Parse(txnRef.Split('-')[0]);

            var rawPhieuIds = HttpContext.Session.GetString("ExamVnPayPhieuIds");
            var parsedPhieuIds = ParsePhieuIds(rawPhieuIds);

            if (!parsedPhieuIds.Any())
            {
                parsedPhieuIds = new List<int> { phieuId };
            }

            string responseCode = vnp.GetResponseData("vnp_ResponseCode");

            if (responseCode == "00")
            {
                var result = _examPaymentService.MarkVnPayExamSuccess(parsedPhieuIds);

                if (result <= 0)
                {
                    ViewBag.Message = "Không tìm thấy phiếu thanh toán khi VNPAY trả về.";
                    return View("PaymentFail");
                }

                await TrySendPaymentInvoiceEmailsAsync(parsedPhieuIds);
                HttpContext.Session.Remove("ExamVnPayPhieuIds");

                TempData["Success"] = "Thanh toán kỳ thi thành công!";
                return View("PaymentSuccess");
            }
            else
            {
                _examPaymentService.MarkExamPaymentFail(parsedPhieuIds);
                HttpContext.Session.Remove("ExamVnPayPhieuIds");

                ViewBag.Message = "Thanh toán thất bại. Mã lỗi: " + responseCode;
                return View("PaymentFail");
            }
        }

        // =========================
        // PAYPAL
        // =========================
        [HttpGet("PayPal")]
        public async Task<IActionResult> PayPal(string phieuIds)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var parsedPhieuIds = ParsePhieuIds(phieuIds);
            if (!parsedPhieuIds.Any())
            {
                TempData["Error"] = "Danh sách phiếu không hợp lệ.";
                return RedirectToAction("Index", "Exam");
            }

            try
            {
                var invoices = _examPaymentService.GetPayPalExamInfo(userId.Value, parsedPhieuIds);

                if (invoices == null || !invoices.Any())
                {
                    TempData["Error"] = "Không tìm thấy phiếu thanh toán.";
                    return RedirectToAction("Index", "Exam");
                }

                decimal vndAmount = invoices.Sum(x => x.TongTien);
                if (vndAmount <= 0)
                {
                    TempData["Error"] = "Số tiền không hợp lệ.";
                    return RedirectToAction("Index", "Exam");
                }

                decimal usd = Math.Round(vndAmount / 24000m, 2);
                if (usd < 0.01m)
                    usd = 0.01m;

                string returnUrl = Url.Action(
                    "PayPalReturn",
                    "ExamPayment",
                    new { phieuIds = string.Join(",", parsedPhieuIds) },
                    Request.Scheme)!;

                string cancelUrl = Url.Action(
                    "PayPalCancel",
                    "ExamPayment",
                    new { phieuIds = string.Join(",", parsedPhieuIds) },
                    Request.Scheme)!;

                string? approval = await _payPalService.CreateOrderAsync(
                    usd,
                    "USD",
                    returnUrl,
                    cancelUrl
                );

                if (approval == null)
                {
                    TempData["Error"] = "Không tạo được đơn PayPal.";
                    return RedirectToAction("Index", "Exam");
                }

                return Redirect(approval);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Exam");
            }
        }

        [HttpGet("PayPalReturn")]
        public async Task<IActionResult> PayPalReturn(string phieuIds, string token)
        {
            var parsedPhieuIds = ParsePhieuIds(phieuIds);
            if (!parsedPhieuIds.Any())
            {
                ViewBag.Message = "Không tìm thấy phiếu thanh toán.";
                return View("PaymentFail");
            }

            bool success = await _payPalService.CaptureOrderAsync(token);

            var invoices = _examPaymentService.GetPayPalExamInfoByPhieuIds(parsedPhieuIds);

            if (invoices == null || !invoices.Any())
            {
                ViewBag.Message = "Không tìm thấy phiếu thanh toán.";
                return View("PaymentFail");
            }

            if (success)
            {
                var result = _examPaymentService.MarkPayPalExamSuccess(parsedPhieuIds);

                if (result <= 0)
                {
                    ViewBag.Message = "Không tìm thấy phiếu thanh toán.";
                    return View("PaymentFail");
                }

                await TrySendPaymentInvoiceEmailsAsync(parsedPhieuIds);
                TempData["Success"] = "Thanh toán PayPal thành công!";
                return View("PaymentSuccess");
            }

            _examPaymentService.MarkExamPaymentFail(parsedPhieuIds);
            ViewBag.Message = "Thanh toán PayPal thất bại hoặc bị hủy.";
            return View("PaymentFail");
        }

        [HttpGet("PayPalCancel")]
        public IActionResult PayPalCancel(string phieuIds)
        {
            var parsedPhieuIds = ParsePhieuIds(phieuIds);

            if (parsedPhieuIds.Any())
                _examPaymentService.MarkExamPaymentFail(parsedPhieuIds);

            ViewBag.Message = "Bạn đã hủy thanh toán PayPal.";
            return View("PaymentFail");
        }
        // =========================
        // MOMO
        // =========================
        [HttpGet("MoMo")]
        public async Task<IActionResult> MoMo(string phieuIds)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var parsedPhieuIds = ParsePhieuIds(phieuIds);
            if (!parsedPhieuIds.Any())
            {
                TempData["Error"] = "Danh sách phiếu không hợp lệ.";
                return RedirectToAction("Index", "Exam");
            }

            try
            {
                var invoices = _examPaymentService.GetMomoExamInfo(userId.Value, parsedPhieuIds);

                if (invoices == null || !invoices.Any())
                {
                    TempData["Error"] = "Không tìm thấy phiếu thanh toán.";
                    return RedirectToAction("Index", "Exam");
                }

                var totalAmount = invoices.Sum(x => x.TongTien);
                if (totalAmount <= 0)
                {
                    TempData["Error"] = "Số tiền thanh toán không hợp lệ.";
                    return RedirectToAction("Index", "Exam");
                }

                var aggregatePayment = new PaymentGatewayDto
                {
                    PhieuId = parsedPhieuIds.First(),
                    TenPhieu = "Thanh toán kỳ thi",
                    NgayLap = DateTime.Now,
                    TongTien = totalAmount,
                    NgayNop = null,
                    PhuongThuc = "MOMO",
                    HoSoId = invoices.First().HoSoId,
                    GhiChu = string.Join(",", parsedPhieuIds),
                    TenHoSo = invoices.First().TenHoSo,
                    HoTenHocVien = invoices.First().HoTenHocVien,
                    KhoaHocId = 0,
                    TenKhoaHoc = string.Join(", ", invoices.Select(x => x.TenKhoaHoc).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()),
                    TenHang = invoices.First().TenHang
                };

                string returnUrl = Url.Action("MoMoReturn", "ExamPayment", new
                {
                    phieuIds = string.Join(",", parsedPhieuIds)
                }, Request.Scheme)!;

                string ipnUrl = Url.Action("MoMoIpn", "ExamPayment", null, Request.Scheme)!;

                string fakeUrl = Url.Action("FakeMoMo", "ExamPayment", new
                {
                    phieuIds = string.Join(",", parsedPhieuIds)
                }, Request.Scheme)!;

                string payUrl = await _momoService.CreatePaymentUrl(aggregatePayment, returnUrl, ipnUrl, fakeUrl);

                return Redirect(payUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Exam");
            }
        }
        [HttpGet("MoMoReturn")]
        public async Task<IActionResult> MoMoReturn(string phieuIds)
        {
            var parsedPhieuIds = ParsePhieuIds(phieuIds);
            if (!parsedPhieuIds.Any())
            {
                ViewBag.Message = "Không tìm thấy phiếu thanh toán.";
                return View("PaymentFail");
            }

            var result = await _momoService.ProcessReturn(Request.Query);

            if (!result.Success)
            {
                _examPaymentService.MarkExamPaymentFail(parsedPhieuIds);
                ViewBag.Message = result.Message;
                return View("PaymentFail");
            }

            var invoices = _examPaymentService.GetMomoExamInfoByPhieuIds(parsedPhieuIds);
            if (invoices == null || !invoices.Any())
            {
                ViewBag.Message = "Không tìm thấy phiếu thanh toán.";
                return View("PaymentFail");
            }

            var updateResult = _examPaymentService.MarkMomoExamSuccess(parsedPhieuIds);
            if (updateResult <= 0)
            {
                ViewBag.Message = "Không tìm thấy phiếu thanh toán.";
                return View("PaymentFail");
            }

            await TrySendPaymentInvoiceEmailsAsync(parsedPhieuIds);
            TempData["Success"] = "Thanh toán MoMo thành công!";
            return View("PaymentSuccess");
        }
        [HttpGet("FakeMoMo")]
        public async Task<IActionResult> FakeMoMo(string phieuIds)
        {
            var parsedPhieuIds = ParsePhieuIds(phieuIds);
            if (!parsedPhieuIds.Any())
            {
                ViewBag.Message = "Không tìm thấy phiếu thanh toán.";
                return View("PaymentFail");
            }

            var result = _examPaymentService.MarkMomoExamSuccess(parsedPhieuIds);

            if (result <= 0)
            {
                ViewBag.Message = "Không tìm thấy phiếu thanh toán.";
                return View("PaymentFail");
            }

            await TrySendPaymentInvoiceEmailsAsync(parsedPhieuIds);
            return View("PaymentSuccess");
        }
        [HttpPost("MoMoIpn")]
        public IActionResult MoMoIpn()
        {
            return Ok();
        }

        [HttpGet("History")]
        public IActionResult History()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            try
            {
                var model = _examPaymentService.GetPaymentHistoryByUser(userId.Value);
                return View("History", model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("History", new List<ExamPaymentHistoryDto>());
            }
        }

        [HttpGet("HistoryDetail")]
        public IActionResult HistoryDetail(int phieuId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            try
            {
                var model = _examPaymentService.GetPaymentHistoryDetail(userId.Value, phieuId);
                if (model == null)
                    return NotFound();

                return View("HistoryDetail", model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("History");
            }
        }

        [HttpGet("DownloadInvoicePdf")]
        public IActionResult DownloadInvoicePdf(int phieuId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            try
            {
                var model = _examPaymentService.GetInvoiceDetail(userId.Value, phieuId);
                if (model == null)
                {
                    TempData["Error"] = "Không tìm thấy hóa đơn hoặc hóa đơn chưa thanh toán.";
                    return RedirectToAction("History");
                }

                var pdfBytes = GenerateInvoicePdfBytes(model);
                var fileName = $"HoaDonKyThi_{model.PhieuId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("History");
            }
        }

        private async Task TrySendPaymentInvoiceEmailsAsync(List<int> phieuIds)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return;

                foreach (var phieuId in phieuIds.Distinct())
                {
                    var invoice = _examPaymentService.GetInvoiceDetail(userId.Value, phieuId);
                    if (invoice == null)
                        continue;

                    if (string.IsNullOrWhiteSpace(invoice.Email))
                        continue;

                    var pdfBytes = GenerateInvoicePdfBytes(invoice);
                    var pdfFileName = $"HoaDonKyThi_{invoice.PhieuId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                    await _emailService.SendPaymentSuccessEmailAsync(
                        invoice.Email,
                        invoice.HoTenHocVien,
                        invoice.PhieuId,
                        invoice.TenKyThi,
                        invoice.TenHang,
                        invoice.PhuongThuc,
                        invoice.TongTien,
                        invoice.NgayNop,
                        pdfBytes,
                        pdfFileName
                    );
                }
            }
            catch
            {
            }
        }

        private byte[] GenerateInvoicePdfBytes(ExamPaymentInvoiceDto model)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var logoPath = Path.Combine(_environment.WebRootPath, "images", "logo", "logo-full.png");
            if (!System.IO.File.Exists(logoPath))
                throw new FileNotFoundException("Không tìm thấy logo trung tâm.", logoPath);

            var paymentLogoPath = GetPaymentLogoPath(model.PhuongThuc);
            if (!string.IsNullOrWhiteSpace(paymentLogoPath) && !System.IO.File.Exists(paymentLogoPath))
                paymentLogoPath = null;

            var document = new ExamPaymentInvoicePdfDocument(
                model,
                DateTime.Now,
                logoPath,
                paymentLogoPath
            );

            return document.GeneratePdf();
        }

        private string? GetPaymentLogoPath(string method)
        {
            var normalized = (method ?? string.Empty).Trim().ToUpper();

            if (normalized == "VNPAY")
                return Path.Combine(_environment.WebRootPath, "images", "logo", "vnpay-logo.png");

            if (normalized == "PAYPAL")
                return Path.Combine(_environment.WebRootPath, "images", "logo", "paypal-logo.png");

            if (normalized == "MOMO")
                return Path.Combine(_environment.WebRootPath, "images", "logo", "momo-logo.png");

            return null;
        }

        private static List<int> ParsePhieuIds(string? phieuIds)
        {
            if (string.IsNullOrWhiteSpace(phieuIds))
                return new List<int>();

            return phieuIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Distinct()
                .ToList();
        }

        private static string RemoveVietnameseSigns(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var ch in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }

            return sb.ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace("đ", "d")
                .Replace("Đ", "D");
        }
    }
}