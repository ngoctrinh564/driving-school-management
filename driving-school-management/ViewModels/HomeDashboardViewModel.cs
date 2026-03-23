namespace driving_school_management.Models
{
    public class HomeDashboardViewModel
    {
        public bool IsLoggedIn { get; set; }

        public int TotalCourses { get; set; }
        public int TotalExams { get; set; }
        public int TotalLicenses { get; set; }

        public List<HomeCourseItem> FeaturedCourses { get; set; } = new();
        public List<HomeProfileItem> MyProfiles { get; set; } = new();
        public List<HomeExamResultItem> MyExamResults { get; set; } = new();
    }

    public class HomeCourseItem
    {
        public int KhoaHocId { get; set; }
        public string TenKhoaHoc { get; set; } = string.Empty;
        public string TenHang { get; set; } = string.Empty;
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string DiaDiem { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public decimal HocPhi { get; set; }
    }

    public class HomeProfileItem
    {
        public int HoSoId { get; set; }
        public string TenHoSo { get; set; } = string.Empty;
        public string TenHang { get; set; } = string.Empty;
        public DateTime? NgayDangKy { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }

    public class HomeExamResultItem
    {
        public string TenBaiThi { get; set; } = string.Empty;
        public string KetQuaDatDuoc { get; set; } = string.Empty;
        public float? TongDiem { get; set; }
        public string TenKyThi { get; set; } = string.Empty;
    }
}