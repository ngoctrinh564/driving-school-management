using System.Collections.Generic;

namespace driving_school_management.Models.DTOs
{
    public class MyHoSoPageDto
    {
        public List<MyHoSoCardDto> Cards { get; set; } = new List<MyHoSoCardDto>();
        public List<HoSoDetailDto> Details { get; set; } = new List<HoSoDetailDto>();
        public string? HocVienAvatarUrl { get; set; }
        public bool CanCreateHoSo { get; set; }
        public List<string> MissingFields { get; set; } = new List<string>();
    }
}