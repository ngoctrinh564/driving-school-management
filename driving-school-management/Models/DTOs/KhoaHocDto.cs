namespace driving_school_management.Models.DTOs
{
    public class KhoaHocDto
    {
        public int KhoaHocId { get; set; }
        public string TenKhoaHoc { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string DiaDiem { get; set; }
        public string TrangThai { get; set; }
        public string TenHang { get; set; }
        public decimal HocPhi { get; set; }
    }
}
