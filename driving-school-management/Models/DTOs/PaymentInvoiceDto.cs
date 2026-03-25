namespace driving_school_management.Models.DTOs
{
    public class PaymentInvoiceDto
    {
        public int PhieuId { get; set; }
        public string TenPhieu { get; set; } = string.Empty;
        public DateTime? NgayLap { get; set; }
        public DateTime? NgayNop { get; set; }
        public decimal TongTien { get; set; }
        public string PhuongThuc { get; set; } = string.Empty;
        public string LoaiPhi { get; set; } = string.Empty;
        public string GhiChu { get; set; } = string.Empty;
        public string TrangThaiThanhToan { get; set; } = string.Empty;

        public int HoSoId { get; set; }
        public string TenHoSo { get; set; } = string.Empty;
        public string TrangThaiHoSo { get; set; } = string.Empty;

        public int HocVienId { get; set; }
        public string HoTenHocVien { get; set; } = string.Empty;
        public string Sdt { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public int KhoaHocId { get; set; }
        public string TenKhoaHoc { get; set; } = string.Empty;
        public string DiaDiem { get; set; } = string.Empty;
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string TrangThaiKhoaHoc { get; set; } = string.Empty;

        public int HangId { get; set; }
        public string TenHang { get; set; } = string.Empty;
        public string LoaiPhuongTien { get; set; } = string.Empty;
        public decimal HocPhi { get; set; }
    }
}