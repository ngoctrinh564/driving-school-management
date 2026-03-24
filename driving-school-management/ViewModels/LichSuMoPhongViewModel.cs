using driving_school_management.Models.DTOs;

namespace driving_school_management.ViewModels
{
    public class LichSuMoPhongViewModel
    {
        public int IdBoDe { get; set; }
        public int IdBaiLam { get; set; }
        public int TongDiem { get; set; }
        public bool KetQua { get; set; }

        public List<TinhHuongItem2> TinhHuongs { get; set; } = new();
        public List<ReviewFlagItem> Flags { get; set; } = new();
    }
}
