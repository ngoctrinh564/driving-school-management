using driving_school_management.Services;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace driving_school_management.Controllers
{
    public class PaymentInvoiceController : Controller
    {
        private readonly PaymentInvoiceService _paymentInvoiceService;
        private readonly IWebHostEnvironment _environment;

        public PaymentInvoiceController(
            PaymentInvoiceService paymentInvoiceService,
            IWebHostEnvironment environment)
        {
            _paymentInvoiceService = paymentInvoiceService;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Download(int phieuId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var model = _paymentInvoiceService.GetInvoiceDetail(userId.Value, phieuId);
            if (model == null)
                return NotFound();

            QuestPDF.Settings.License = LicenseType.Community;

            var logoPath = Path.Combine(_environment.WebRootPath, "images", "logo", "logo-full.png");
            if (!System.IO.File.Exists(logoPath))
                return BadRequest("Không tìm thấy logo trung tâm.");

            var paymentLogoPath = GetPaymentLogoPath(model.PhuongThuc);
            if (!string.IsNullOrWhiteSpace(paymentLogoPath) && !System.IO.File.Exists(paymentLogoPath))
                paymentLogoPath = null;

            var document = new PaymentInvoicePdfDocument(
                model,
                DateTime.Now,
                logoPath,
                paymentLogoPath
            );

            var pdfBytes = document.GeneratePdf();

            var fileName = $"HoaDonThanhToan_{model.PhieuId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
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