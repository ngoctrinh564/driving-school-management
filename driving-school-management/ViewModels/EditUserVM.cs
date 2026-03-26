using System;
using System.ComponentModel.DataAnnotations;

namespace driving_school_management.ViewModels
{
    public class EditUserVM
    {
        public int UserId { get; set; }
        public int HocVienId { get; set; }

        [Required]
        public string Username { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        public string HoTen { get; set; } = "";
        public string SoCmndCccd { get; set; } = "";
        public DateTime? NamSinh { get; set; }
        public string GioiTinh { get; set; } = "";
        public string Sdt { get; set; } = "";
        public string? AvatarUrl { get; set; } = "";
    }
}