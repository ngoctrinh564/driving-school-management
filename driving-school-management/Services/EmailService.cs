using System.Net;
using System.Net.Mail;

namespace driving_school_management.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        {
            var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "";
            var smtpPortText = _configuration["EmailSettings:SmtpPort"] ?? "587";
            var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "";
            var senderPassword = _configuration["EmailSettings:SenderPassword"] ?? "";
            var senderName = _configuration["EmailSettings:SenderName"] ?? "";

            int smtpPort = int.Parse(smtpPortText);

            using var message = new MailMessage();
            message.From = new MailAddress(senderEmail, senderName);
            message.To.Add(toEmail);
            message.Subject = "Mã OTP xác nhận đăng ký - GPLX System";

            message.Body = $@"
            <div style='font-family:Arial, sans-serif; background:#f6f6f6; padding:20px'>
                <div style='max-width:500px; margin:auto; background:#ffffff; border-radius:10px; padding:20px; text-align:center; box-shadow:0 2px 10px rgba(0,0,0,0.1)'>

                    <h2 style='color:#198754;'>GPLX System</h2>

                    <p style='font-size:16px; color:#333;'>Mã OTP xác nhận đăng ký của bạn là:</p>

                    <div style='font-size:32px; font-weight:bold; color:#198754; margin:20px 0'>
                        {otpCode}
                    </div>

                    <p style='color:#555; font-size:14px'>
                        Mã có hiệu lực trong <strong>60 giây</strong>.
                    </p>

                    <hr style='margin:20px 0'/>

                    <p style='font-size:12px; color:#888'>
                        Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.
                    </p>

                </div>
            </div>
            ";

                        message.IsBodyHtml = true;

            using var client = new SmtpClient(smtpHost, smtpPort);
            client.Credentials = new NetworkCredential(senderEmail, senderPassword);
            client.EnableSsl = true;

            await client.SendMailAsync(message);
        }
    }
}