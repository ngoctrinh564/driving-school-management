namespace driving_school_management.Models.DTOs
{
    public class KhoaHocDangKyCheckDto
    {
        public int KhoaHocId { get; set; }
        public string TenKhoaHoc { get; set; } = string.Empty;
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string DiaDiem { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;

        public int HangId { get; set; }
        public string TenHang { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public string LoaiPhuongTien { get; set; } = string.Empty;
        public int SoCauHoi { get; set; }
        public int DiemDat { get; set; }
        public int ThoiGianTn { get; set; }
        public decimal HocPhi { get; set; }

        public int IsMoDangKy { get; set; }
        public int HasHoSoPhuHop { get; set; }
        public int HoSoIdPhuHop { get; set; }
        public string TenHoSoPhuHop { get; set; } = string.Empty;
        public DateTime? NgayDangKyHoSo { get; set; }
        public string TrangThaiHoSo { get; set; } = string.Empty;
        public int DaTungHocHang { get; set; }
        public int SoLanDaHocHang { get; set; }
        public int HasHoSoChuaDuyet { get; set; }
    }
}
