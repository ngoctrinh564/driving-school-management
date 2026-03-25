using System.Threading.Tasks;

namespace driving_school_management.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otpCode);
    }
}