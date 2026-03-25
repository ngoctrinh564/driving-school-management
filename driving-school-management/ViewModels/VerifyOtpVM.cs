using System.ComponentModel.DataAnnotations;

namespace driving_school_management.ViewModels
{
    public class VerifyOtpVM
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public string OtpCode { get; set; }
    }
}