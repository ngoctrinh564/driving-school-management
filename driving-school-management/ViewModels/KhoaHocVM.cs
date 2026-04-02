namespace driving_school_management.ViewModels
{
    public class KhoaHocVM
    {
        public int STT { get; set; }

        public int KhoaHocId { get; set; }

        public string TenKhoaHoc { get; set; }

        public int HangId { get; set; }

        public string TenHang { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime NgayKetThuc { get; set; }

        public string TrangThai { get; set; }
    }
}
