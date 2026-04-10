namespace driving_school_management.Models.DTOs
{
    public class UserExamConfirmDto
    {
        public string? HoTen { get; set; }
        public int HoSoId { get; set; }
        public int HangId { get; set; }
        public string? MaHang { get; set; }
        public string? TenHang { get; set; }
        public decimal HocPhi { get; set; }
        public decimal LePhiMotKy { get; set; }
        public decimal TongPhi { get; set; }
        public bool DuDieuKienThiTotNghiep { get; set; }
        public bool DuDieuKienThiSatHach { get; set; }
        public bool DauTotNghiep { get; set; }
        public bool DaHoanThanh { get; set; }
        public List<UserExamConfirmItemDto> DanhSachKyThi { get; set; } = new();
    }
}