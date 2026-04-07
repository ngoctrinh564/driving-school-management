namespace driving_school_management.Models.DTOs
{
    public class HocVienDangKyThiDto
    {
        public int KyThiId { get; set; }
        public int HoSoId { get; set; }
        public int HocVienId { get; set; }
        public string? HoTen { get; set; }
        public string? Sdt { get; set; }
        public string? Email { get; set; }
        public int? LichThiId { get; set; }
        public int PhieuId { get; set; }
        public string? TenPhieu { get; set; }
    }
}
