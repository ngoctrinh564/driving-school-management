namespace driving_school_management.Models.DTOs
{
    public class MyHoSoCardDto
    {
        public int HoSoId { get; set; }
        public string HoTen { get; set; }
        public string AvatarUrl { get; set; }
        public string TenHoSo { get; set; }
        public string TenHang { get; set; }
        public DateTime NgayDangKy { get; set; }
        public string TrangThai { get; set; }
        public int SoThangConLai { get; set; }
        public int SoNgayConLai { get; set; }
    }
}
