namespace driving_school_management.ViewModels
{
    public class HocAllChapterVM
    {
        public int ChuongId { get; set; }
        public string TenChuong { get; set; } = "";
        public int ThuTu { get; set; }

        public List<HocAllQuestionVM> Questions { get; set; } = new();
    }
}