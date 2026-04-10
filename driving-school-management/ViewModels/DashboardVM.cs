using driving_school_management.Models;

namespace driving_school_management.ViewModels
{
    public class DashboardVM
    {
        public int TotalUser { get; set; }
        public int TotalHoSo { get; set; }
        public int TotalGPLX { get; set; }
        public int TotalBaiThi { get; set; }

        public int UserActive { get; set; }
        public int UserInactive { get; set; }

        public int HoSoDaDuyet { get; set; }
        public int HoSoDangXuLy { get; set; }

        public List<string> Thang { get; set; } = new();
        public List<int> SoHoSoTheoThang { get; set; } = new();
        public List<decimal> DoanhThuTheoThang { get; set; } = new();

        public List<string> HangLabels { get; set; } = new();
        public List<int> HangData { get; set; } = new();

        public List<UserVM> RecentUsers { get; set; } = new();
    }
}