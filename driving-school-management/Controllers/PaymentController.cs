using driving_school_management.Librarys;
using driving_school_management.Models.DTOs;
using driving_school_management.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using System.Globalization;
using System.Text;

namespace driving_school_management.Controllers
{
    [Route("Payment")]
    public class PaymentController : Controller
    {
        private readonly PaymentService _paymentService;
        private readonly PaymentInvoiceService _paymentInvoiceService;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _config;
        private readonly IPayPalService _payPalService;
        private readonly IMomoService _momoService;

        public PaymentController(
            PaymentService paymentService,
            PaymentInvoiceService paymentInvoiceService,
            IEmailService emailService,
            IWebHostEnvironment environment,
            IConfiguration config,
            IPayPalService payPalService,
            IMomoService momoService)
        {
            _paymentService = paymentService;
            _paymentInvoiceService = paymentInvoiceService;
            _emailService = emailService;
            _environment = environment;
            _config = config;
            _payPalService = payPalService;
            _momoService = momoService;
        }

        // ============================================================
        // 1) TRANG BẮT ĐẦU THANH TOÁN
        // ============================================================
        [HttpGet("StartPayment")]
        public IActionResult StartPayment(int khoaHocId, int hoSoId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var model = _paymentService.StartPayment(userId.Value, hoSoId, khoaHocId);

            if (model == null)
            {
                TempData["Error"] = "Không tìm thấy dữ liệu thanh toán.";
                return RedirectToAction("Index", "KhoaHoc");
            }

            if (model.IsValid == 0)
            {
                TempData["Error"] = model.Message;
                return RedirectToAction("Index", "KhoaHoc");
            }

            return View("StartPayment", model);
        }

        // ============================================================
        // 2) CHỌN PHƯƠNG THỨC THANH TOÁN
        // ============================================================
        [HttpPost("ChoosePaymentMethod")]
        [ValidateAntiForgeryToken]
        public IActionResult ChoosePaymentMethod(int phieuId, string method, string noiDung)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var result = _paymentService.ChoosePaymentMethod(userId.Value, phieuId, method, noiDung);

            if (result == -1)
            {
                TempData["Error"] = "Không tìm thấy phiếu thanh toán.";
                return RedirectToAction("Index", "KhoaHoc");
            }

            if (method == "VNPAY")
                return RedirectToAction("VnPay", new { phieuId });

            if (method == "PAYPAL")
                return RedirectToAction("PayPal", new { phieuId });

            if (method == "MOMO")
                return RedirectToAction("MoMo", new { phieuId });

            TempData["Error"] = "Phương thức thanh toán chưa được hỗ trợ.";
            return RedirectToAction("Index", "KhoaHoc");
        }

        // ============================================================
        // TẠO URL THANH TOÁN VNPAY
        // ============================================================
        [HttpGet("VnPay")]
        public IActionResult VnPay(int phieuId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var hd = _paymentService.GetVnPayInfo(userId.Value, phieuId);

            if (hd == null)
            {
                TempData["Error"] = "Không tìm thấy phiếu thanh toán.";
                return RedirectToAction("Index", "KhoaHoc");
            }

            var amountDecimal = hd.TongTien;
            if (amountDecimal <= 0)
            {
                TempData["Error"] = "Số tiền thanh toán không hợp lệ.";
                return RedirectToAction("StartPayment", new { khoaHocId = hd.KhoaHocId, hoSoId = hd.HoSoId });
            }

            long amount = (long)amountDecimal;

            string baseUrl = _config["VnPay:BaseUrl"]!;
            string tmnCode = _config["VnPay:TmnCode"]!;
            string hashSecret = _config["VnPay:HashSecret"]!;
            string orderType = _config["VnPay:OrderType"] ?? "other";
            string locale = _config["VnPay:Locale"] ?? "vn";
            string currCode = _config["VnPay:CurrCode"] ?? "VND";

            string returnUrl = Url.Action("VnPayReturn", "Payment", null, Request.Scheme)!;

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

            string infoRaw = string.IsNullOrWhiteSpace(hd.GhiChu)
                ? $"Thanh toan khoa hoc {hd.TenKhoaHoc}"
                : hd.GhiChu;

            vnp.AddRequestData("vnp_OrderInfo", RemoveVietnameseSigns(infoRaw));
            vnp.AddRequestData("vnp_OrderType", orderType);
            vnp.AddRequestData("vnp_ReturnUrl", returnUrl);

            string txnRef = $"{phieuId}-{DateTime.Now:yyyyMMddHHmmss}";
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

            string responseCode = vnp.GetResponseData("vnp_ResponseCode");

            if (responseCode == "00")
            {
                var result = _paymentService.MarkVnPaySuccess(phieuId);

                if (result == -1)
                {
                    ViewBag.Message = "Không tìm thấy phiếu thanh toán khi VNPAY trả về.";
                    return View("PaymentFail");
                }

                await TrySendPaymentInvoiceEmailAsync(phieuId);

                TempData["Success"] = "Thanh toán thành công!";
                return View("PaymentSuccess");
            }
            else
            {
                _paymentService.MarkVnPayFail(phieuId);
                ViewBag.Message = "Thanh toán thất bại. Mã lỗi: " + responseCode;
                return View("PaymentFail");
            }
        }

        private static string RemoveVietnameseSigns(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

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
        // ============================================================
        // TẠO URL THANH TOÁN PAYPAL
        // ============================================================
        [HttpGet("PayPal")]
        public async Task<IActionResult> PayPal(int phieuId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var hd = _paymentService.GetPayPalInfo(userId.Value, phieuId);

            if (hd == null)
            {
                TempData["Error"] = "Không tìm thấy phiếu thanh toán.";
                return RedirectToAction("Index", "KhoaHoc");
            }

            decimal vndAmount = hd.TongTien;
            if (vndAmount <= 0)
            {
                TempData["Error"] = "Số tiền không hợp lệ.";
                return RedirectToAction("StartPayment", new { khoaHocId = hd.KhoaHocId, hoSoId = hd.HoSoId });
            }

            decimal usd = Math.Round(vndAmount / 24000m, 2);
            if (usd < 0.01m)
                usd = 0.01m;

            string returnUrl = Url.Action("PayPalReturn", "Payment", new { phieuId }, Request.Scheme)!;
            string cancelUrl = Url.Action("PayPalCancel", "Payment", new { phieuId }, Request.Scheme)!;

            string? approval = await _payPalService.CreateOrderAsync(usd, "USD", returnUrl, cancelUrl);

            if (approval == null)
            {
                TempData["Error"] = "Không tạo được đơn PayPal.";
                return RedirectToAction("StartPayment", new { khoaHocId = hd.KhoaHocId, hoSoId = hd.HoSoId });
            }

            return Redirect(approval);
        }

        [HttpGet("PayPalReturn")]
        public async Task<IActionResult> PayPalReturn(int phieuId, string token)
        {
            bool success = await _payPalService.CaptureOrderAsync(token);

            var hd = _paymentService.GetPayPalInfoFromPhieuIdForReturn(phieuId);

            if (hd == null)
            {
                ViewBag.Message = "Không tìm thấy phiếu thanh toán.";
                return View("PaymentFail");
            }

            if (success)
            {
                var result = _paymentService.MarkPayPalSuccess(phieuId);

                if (result == -1)
                {
                    ViewBag.Message = "Không tìm thấy phiếu thanh toán.";
                    return View("PaymentFail");
                }

                await TrySendPaymentInvoiceEmailAsync(phieuId);
                TempData["Success"] = "Thanh toán PayPal thành công!";
                return View("PaymentSuccess");
            }

            _paymentService.MarkPayPalFail(phieuId);
            ViewBag.Message = "Thanh toán PayPal thất bại hoặc bị hủy.";
            return View("PaymentFail");
        }

        [HttpGet("PayPalCancel")]
        public IActionResult PayPalCancel(int phieuId)
        {
            _paymentService.MarkPayPalFail(phieuId);
            ViewBag.Message = "Bạn đã hủy thanh toán PayPal.";
            return View("PaymentFail");
        }
        // ============================================================
        // TẠO URL THANH TOÁN MOMO
        // ============================================================
        [HttpGet("MoMo")]
        public async Task<IActionResult> MoMo(int phieuId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var hd = _paymentService.GetMomoInfo(userId.Value, phieuId);

            if (hd == null)
            {
                TempData["Error"] = "Không tìm thấy phiếu thanh toán.";
                return RedirectToAction("Index", "KhoaHoc");
            }

            if (hd.TongTien <= 0)
            {
                TempData["Error"] = "Số tiền thanh toán không hợp lệ.";
                return RedirectToAction("StartPayment", new { khoaHocId = hd.KhoaHocId, hoSoId = hd.HoSoId });
            }

            string returnUrl = Url.Action("MoMoReturn", "Payment", null, Request.Scheme)!;
            string ipnUrl = Url.Action("MoMoIpn", "Payment", null, Request.Scheme)!;
            string fakeUrl = Url.Action("FakeMoMo", "Payment", new { phieuId }, Request.Scheme)!;

            string payUrl = await _momoService.CreatePaymentUrl(hd, returnUrl, ipnUrl, fakeUrl);

            return Redirect(payUrl);
        }

        [HttpGet("MoMoReturn")]
        public async Task<IActionResult> MoMoReturn()
        {
            var result = await _momoService.ProcessReturn(Request.Query);

            if (!result.Success)
            {
                ViewBag.Message = result.Message;
                return View("PaymentFail");
            }

            if (result.PhieuId == null)
            {
                ViewBag.Message = "Không tìm thấy phiếu thanh toán.";
                return View("PaymentFail");
            }

            var phieu = _paymentService.GetMomoInfoByPhieuId(result.PhieuId.Value);
            if (phieu == null)
            {
                ViewBag.Message = "Không tìm thấy phiếu thanh toán.";
                return View("PaymentFail");
            }

            var updateResult = _paymentService.MarkMomoSuccess(result.PhieuId.Value);
            if (updateResult == -1)
            {
                ViewBag.Message = "Không tìm thấy phiếu thanh toán.";
                return View("PaymentFail");
            }

            await TrySendPaymentInvoiceEmailAsync(result.PhieuId.Value);
            return View("PaymentSuccess");
        }

        [HttpPost("MoMoIpn")]
        public IActionResult MoMoIpn()
        {
            return Ok();
        }

        [HttpGet("FakeMoMo")]
        public async Task<IActionResult> FakeMoMo(int phieuId)
        {
            var result = _paymentService.MarkMomoSuccess(phieuId);

            if (result == -1)
            {
                ViewBag.Message = "Không tìm thấy phiếu thanh toán.";
                return View("PaymentFail");
            }

            await TrySendPaymentInvoiceEmailAsync(phieuId);
            return View("PaymentSuccess");
        }

        // Gửi mail
        private async Task TrySendPaymentInvoiceEmailAsync(int phieuId)
        {
            try
            {
                var invoice = GetInvoiceDetailForCurrentUser(phieuId);
                if (invoice == null)
                    return;

                if (string.IsNullOrWhiteSpace(invoice.Email))
                    return;

                var pdfBytes = GenerateInvoicePdfBytes(invoice);
                var pdfFileName = $"HoaDonThanhToan_{invoice.PhieuId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                await _emailService.SendPaymentSuccessEmailAsync(
                    invoice.Email,
                    invoice.HoTenHocVien,
                    invoice.PhieuId,
                    invoice.TenKhoaHoc,
                    invoice.TenHang,
                    invoice.PhuongThuc,
                    invoice.TongTien,
                    invoice.NgayNop,
                    pdfBytes,
                    pdfFileName
                );
            }
            catch
            {
            }
        }

        private PaymentInvoiceDto? GetInvoiceDetailForCurrentUser(int phieuId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return null;

            return _paymentInvoiceService.GetInvoiceDetail(userId.Value, phieuId);
        }

        private byte[] GenerateInvoicePdfBytes(PaymentInvoiceDto model)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var logoPath = Path.Combine(_environment.WebRootPath, "images", "logo", "logo-full.png");
            if (!System.IO.File.Exists(logoPath))
                throw new FileNotFoundException("Không tìm thấy logo trung tâm.", logoPath);

            var paymentLogoPath = GetPaymentLogoPath(model.PhuongThuc);
            if (!string.IsNullOrWhiteSpace(paymentLogoPath) && !System.IO.File.Exists(paymentLogoPath))
                paymentLogoPath = null;

            var document = new PaymentInvoicePdfDocument(
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
    }
}