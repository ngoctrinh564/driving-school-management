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
        public int? SoCauHoi { get; set; }
        public int? DiemDat { get; set; }
        public int? ThoiGianTn { get; set; }
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

        public int BiTrungThoiGianHoc { get; set; }
        public int KhoaHocIdTrungThoiGian { get; set; }
        public string TenKhoaHocTrungThoiGian { get; set; } = string.Empty;
        public DateTime? NgayBatDauTrungThoiGian { get; set; }
        public DateTime? NgayKetThucTrungThoiGian { get; set; }

        public int DaTungDangKyCungHang { get; set; }
        public int KhoaHocIdCungHangGanNhat { get; set; }
        public string TenKhoaHocCungHangGanNhat { get; set; } = string.Empty;

        public int CoTheDangKy { get; set; }

        public int DaDangKyChinhKhoaHoc { get; set; }
        public int KhoaHocIdDaDangKy { get; set; }
        public string TenKhoaHocDaDangKy { get; set; } = string.Empty;
        public int TongHoSo { get; set; }
        public int TongHoSoConHan { get; set; }
        public int TongHoSoCungHang { get; set; }
        public int TongHoSoDaDuyetConHan { get; set; }
        public int TongHoSoDangXuLyConHan { get; set; }
        public int TongHoSoBiLoaiConHan { get; set; }
        public int TongHoSoHetHan { get; set; }

        public string StatusCode { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
    }
}