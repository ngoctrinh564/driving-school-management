namespace driving_school_management.ViewModels
{
    public class ThiTrialViewModel
    {
        public int IdBoDe { get; set; }
        public List<TinhHuongItem2> TinhHuongs { get; set; } = new();
    }

    public class TinhHuongItem2
    {
        public int IdThMp { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public double ScoreStartSec { get; set; }
        public double ScoreEndSec { get; set; }
        public string HintImageUrl { get; set; } = string.Empty;
        public bool Kho { get; set; }
        public List<MocDiemItem> Mocs { get; set; } = new();
    }

    public class MocDiemItem
    {
        public int Diem { get; set; }
        public double TimeSec { get; set; }
    }
}
