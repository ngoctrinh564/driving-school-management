namespace driving_school_management.ViewModels
{
    public class KetQuaThiViewModel
    {
        public int TongDiem { get; set; }
        public bool KetQua { get; set; }
        public List<ChiTietKetQuaItem> ChiTiet { get; set; } = new();
    }

    public class ChiTietKetQuaItem
    {
        public string TieuDe { get; set; } = string.Empty;
        public double ThoiDiemNhan { get; set; }
        public int Diem { get; set; }
    }
}
