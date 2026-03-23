namespace driving_school_management.ViewModels
{
    public class HocAllChapterVM
    {
        public int ChuongId { get; set; }
        public string TenChuong { get; set; } = string.Empty;
        public int ThuTu { get; set; }
        public List<HocAllQuestionVM> Questions { get; set; } = new();
    }
}