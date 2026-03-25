using System;

namespace driving_school_management.ViewModels
{
    public class RegisterOtpSessionVM
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public string OtpCode { get; set; }
        public DateTime ExpiredAt { get; set; }
    }
}