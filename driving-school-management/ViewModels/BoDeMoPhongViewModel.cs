namespace driving_school_management.ViewModels
{
    public class BoDeMoPhongViewModel
    {
        public int IdBoDe { get; set; }
        public string TenBoDe { get; set; } = string.Empty;
        public int SoTinhHuong { get; set; }

        public bool HasResult { get; set; }
        public int TongDiem { get; set; }
        public bool KetQua { get; set; }
        public int SoTinhHuongSai { get; set; }
        public int? IdBaiLamMoiNhat { get; set; }
    }
}
