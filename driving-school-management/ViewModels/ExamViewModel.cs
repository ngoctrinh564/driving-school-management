namespace driving_school_management.ViewModels
{
    public class ExamViewModel
    {
        public int IdBoDe { get; set; }
        public string TenBoDe { get; set; } = string.Empty;
        public string Hang { get; set; } = string.Empty;
        public int ThoiGian { get; set; }
        public int ThoiGianLam { get; set; }
        public int TongCau { get; set; }
        public int DiemDat { get; set; }
        public int SoCauDung { get; set; }
        public int SoCauSai { get; set; }
        public bool Dat { get; set; }
        public bool CoCauLietSai { get; set; }
        public bool IsSubmitted { get; set; }
        public bool IsRandomExam { get; set; }
        public List<ExamQuestionVM> CauHoi { get; set; } = new();
        public Dictionary<int, int?> DapAnDaChon { get; set; } = new();
        public List<string> DanhSachMeo { get; set; } = new();
    }
}