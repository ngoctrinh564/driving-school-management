namespace driving_school_management.Models.DTOs
{
    public class PaymentGatewayDto
    {
        public int PhieuId { get; set; }
        public string TenPhieu { get; set; } = string.Empty;
        public DateTime? NgayLap { get; set; }
        public decimal TongTien { get; set; }
        public DateTime? NgayNop { get; set; }
        public string PhuongThuc { get; set; } = string.Empty;

        public int HoSoId { get; set; }
        public string GhiChu { get; set; } = string.Empty;
        public string TenHoSo { get; set; } = string.Empty;
        public string HoTenHocVien { get; set; } = string.Empty;

        public int KhoaHocId { get; set; }
        public string TenKhoaHoc { get; set; } = string.Empty;
        public string TenHang { get; set; } = string.Empty;
    }
}