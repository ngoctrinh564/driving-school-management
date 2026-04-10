namespace driving_school_management.Models.DTOs
{
    public class UserExamDto
    {
        public int KyThiId { get; set; }
        public string? TenKyThi { get; set; }
        public string? LoaiKyThi { get; set; }
        public int HoSoId { get; set; }
        public int HangId { get; set; }
        public string? MaHang { get; set; }
        public string? TenHang { get; set; }
        public decimal HocPhi { get; set; }
        public bool DuDieuKienThiTotNghiep { get; set; }
        public bool DuDieuKienThiSatHach { get; set; }
        public bool DauTotNghiep { get; set; }
        public bool DaHoanThanh { get; set; }
        public bool CoTheDangKy { get; set; }
        public int SoKyThiDangKy { get; set; }
        public decimal TongPhiDuKien { get; set; }
        public string? CanhBao { get; set; }
    }
}