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

        public List<UserVM> RecentUsers { get; set; } = new List<UserVM>();
    }
}
