namespace driving_school_management.ViewModels
{
    public class UserDashboardVM
    {
        public int UserId { get; set; }
        public int HocVienId { get; set; }

        public string Username { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
        public string Sdt { get; set; } = "";
        public string GioiTinh { get; set; } = "";
        public string AvatarUrl { get; set; } = "";

        public string SoCmndCccd { get; set; } = "";
        public DateTime? NamSinh { get; set; }

        public string TenHang { get; set; } = "";
        public string HoSoTrangThai { get; set; } = "";
        public DateTime? NgayDangKy { get; set; }

        public int SoBuoiHoc { get; set; }
        public int SoKyThi { get; set; }
        public decimal TongThanhToan { get; set; }

        public string GplxTrangThai { get; set; } = "";
    }
}
