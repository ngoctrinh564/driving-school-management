namespace driving_school_management.Models.DTOs
{
    public class MyCourseDto
    {
        public int KhoaHocId { get; set; }
        public string TenKhoaHoc { get; set; } = string.Empty;
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string DiaDiem { get; set; } = string.Empty;
        public string TrangThaiKhoaHocGoc { get; set; } = string.Empty;

        public int HangId { get; set; }
        public string TenHang { get; set; } = string.Empty;
        public string LoaiPhuongTien { get; set; } = string.Empty;
        public decimal HocPhi { get; set; }

        public int HoSoId { get; set; }
        public string TenHoSo { get; set; } = string.Empty;
        public int HocVienId { get; set; }
        public string HoTenHocVien { get; set; } = string.Empty;

        public int PhieuId { get; set; }
        public string TenPhieu { get; set; } = string.Empty;
        public DateTime? NgayLap { get; set; }
        public DateTime? NgayNop { get; set; }
        public decimal TongTien { get; set; }
        public string PhuongThuc { get; set; } = string.Empty;
        public string LoaiPhi { get; set; } = string.Empty;
        public string GhiChu { get; set; } = string.Empty;

        public int KetQuaHocTapId { get; set; }
        public int? LyThuyetKq { get; set; }
        public int? SaHinhKq { get; set; }
        public int? DuongTruongKq { get; set; }
        public int? MoPhongKq { get; set; }

        public string TrangThaiHocTap { get; set; } = string.Empty;
        public int DaHoanThanh { get; set; }
        public int DangHoc { get; set; }
        public int KhongHoanThanh { get; set; }
    }
}