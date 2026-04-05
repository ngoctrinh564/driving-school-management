using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace driving_school_management.ViewModels
{
    public class SignVM
    {
        public int IDBIENBAO { get; set; }

        [Required(ErrorMessage = "Tên biển báo không được để trống")]
        public string TENBIENBAO { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ý nghĩa không được để trống")]
        public string YNGHIA { get; set; } = string.Empty;

        public string? HINHANH { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}