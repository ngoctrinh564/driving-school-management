using driving_school_management.Helpers;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Net.Mail;

namespace driving_school_management.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otpCode);
        Task SendNotificationEmailAsync(string toEmail, string subject, string title, string content);
        Task SendPaymentSuccessEmailAsync(
            string toEmail,
            string hoTen,
            int phieuId,
            string tenKhoaHoc,
            string tenHang,
            string phuongThuc,
            decimal tongTien,
            DateTime? ngayNop,
            byte[] pdfBytes,
            string pdfFileName);
    }

    public class EmailService : IEmailService
    {
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly EmailSettings _emailSettings;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            IEmailTemplateService emailTemplateService)
        {
            _emailSettings = emailSettings.Value;
            _emailTemplateService = emailTemplateService;
        }

        private SmtpClient CreateSmtpClient()
        {
            return new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(
                    _emailSettings.SenderEmail,
                    _emailSettings.SenderPassword
                ),
                EnableSsl = true
            };
        }

        private MailMessage CreateMailMessage(string toEmail, string subject, string body)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);
            return message;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        {
            var template = await _emailTemplateService.GetTemplateAsync("OtpEmailTemplate.html");

            var body = _emailTemplateService.ReplacePlaceholders(template, new Dictionary<string, string>
            {
                { "{{OTP_CODE}}", otpCode },
                { "{{EXPIRE_SECONDS}}", "60 giây" }
            });

            using var message = CreateMailMessage(
                toEmail,
                "Mã OTP xác nhận đăng ký - GPLX System",
                body
            );

            using var client = CreateSmtpClient();
            await client.SendMailAsync(message);
        }

        public async Task SendNotificationEmailAsync(string toEmail, string subject, string title, string content)
        {
            var template = await _emailTemplateService.GetTemplateAsync("NotificationEmailTemplate.html");

            var body = _emailTemplateService.ReplacePlaceholders(template, new Dictionary<string, string>
            {
                { "{{EMAIL_SUBJECT}}", subject },
                { "{{TITLE}}", title },
                { "{{CONTENT}}", content }
            });

            using var message = CreateMailMessage(toEmail, subject, body);

            using var client = CreateSmtpClient();
            await client.SendMailAsync(message);
        }

        public async Task SendPaymentSuccessEmailAsync(
            string toEmail,
            string hoTen,
            int phieuId,
            string tenKhoaHoc,
            string tenHang,
            string phuongThuc,
            decimal tongTien,
            DateTime? ngayNop,
            byte[] pdfBytes,
            string pdfFileName)
        {
            var template = await _emailTemplateService.GetTemplateAsync("PaymentSuccessEmailTemplate.html");

            var body = _emailTemplateService.ReplacePlaceholders(template, new Dictionary<string, string>
            {
                { "{{EMAIL_SUBJECT}}", "Thông báo thanh toán thành công - GPLX System" },
                { "{{TITLE}}", "Thanh toán của bạn đã được xác nhận" },
                { "{{HO_TEN}}", string.IsNullOrWhiteSpace(hoTen) ? "Học viên" : hoTen },
                { "{{PHIEU_ID}}", phieuId.ToString() },
                { "{{TEN_KHOA_HOC}}", tenKhoaHoc ?? string.Empty },
                { "{{TEN_HANG}}", tenHang ?? string.Empty },
                { "{{PHUONG_THUC}}", phuongThuc ?? string.Empty },
                { "{{TONG_TIEN}}", string.Format(new CultureInfo("vi-VN"), "{0:N0} VNĐ", tongTien) },
                { "{{NGAY_NOP}}", ngayNop.HasValue ? ngayNop.Value.ToString("dd/MM/yyyy HH:mm:ss") : "Chưa cập nhật" }
            });

            using var message = CreateMailMessage(
                toEmail,
                "Thông báo thanh toán thành công - GPLX System",
                body
            );

            using var attachmentStream = new MemoryStream(pdfBytes);
            var attachment = new Attachment(attachmentStream, pdfFileName, "application/pdf");
            message.Attachments.Add(attachment);

            using var client = CreateSmtpClient();
            await client.SendMailAsync(message);
        }
    }
}