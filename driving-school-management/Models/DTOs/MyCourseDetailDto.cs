namespace driving_school_management.Models.DTOs
{
    public class MyCourseDetailDto
    {
        public int KhoaHocId { get; set; }
        public string TenKhoaHoc { get; set; } = string.Empty;
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string DiaDiem { get; set; } = string.Empty;
        public string TrangThaiKhoaHocGoc { get; set; } = string.Empty;

        public int HangId { get; set; }
        public string TenHang { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public string LoaiPhuongTien { get; set; } = string.Empty;
        public int? SoCauHoi { get; set; }
        public int? DiemDat { get; set; }
        public int? ThoiGianTn { get; set; }
        public decimal HocPhi { get; set; }

        public int HocVienId { get; set; }
        public string HoTenHocVien { get; set; } = string.Empty;
        public string Sdt { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public int HoSoId { get; set; }
        public string TenHoSo { get; set; } = string.Empty;
        public DateTime? NgayDangKy { get; set; }
        public string TrangThaiHoSo { get; set; } = string.Empty;
        public string GhiChuHoSo { get; set; } = string.Empty;

        public int PhieuId { get; set; }
        public string TenPhieu { get; set; } = string.Empty;
        public DateTime? NgayLap { get; set; }
        public DateTime? NgayNop { get; set; }
        public decimal TongTien { get; set; }
        public string PhuongThuc { get; set; } = string.Empty;
        public string LoaiPhi { get; set; } = string.Empty;
        public string GhiChuThanhToan { get; set; } = string.Empty;

        public int KetQuaHocTapId { get; set; }
        public string NhanXet { get; set; } = string.Empty;
        public int? SoBuoiHoc { get; set; }
        public int? SoBuoiVang { get; set; }
        public string SoKmHoanThanh { get; set; } = string.Empty;

        public int? LyThuyetKq { get; set; }
        public int? SaHinhKq { get; set; }
        public int? DuongTruongKq { get; set; }
        public int? MoPhongKq { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int IsActive { get; set; }
        public string TrangThaiHocTap { get; set; } = string.Empty;
    }
}