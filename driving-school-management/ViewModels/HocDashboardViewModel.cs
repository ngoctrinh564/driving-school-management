namespace driving_school_management.ViewModels
{
    public class HocDashboardViewModel
    {
        public List<string> ListHang { get; set; } = new();

        public string? SelectedHang { get; set; }
        public bool ShowPopup { get; set; }

        public int ThoiGianThi { get; set; }
        public int SoCauThiNgauNhien { get; set; }

        public int TotalBoDe { get; set; }
        public int DoneBoDe { get; set; }

        public int TotalCauHoi { get; set; }
        public int TotalCauLiet { get; set; }
        public int TotalCauChuY { get; set; }
        public int TotalBienBao { get; set; }

        public bool HasMoPhong { get; set; }
        public int MpBoDe { get; set; }
        public int MpTinhHuong { get; set; }
        public int MpBoDeDone { get; set; }
    }
}