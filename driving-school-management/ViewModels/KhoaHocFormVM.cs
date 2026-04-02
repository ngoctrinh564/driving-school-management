namespace driving_school_management.ViewModels
{
    public class KhoaHocFormVM
    {
        public int KhoaHocId { get; set; }
        public int HangId { get; set; }
        public string TenKhoaHoc { get; set; } = string.Empty;
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string DiaDiem { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public string TenHang { get; set; } = string.Empty;
    }
}