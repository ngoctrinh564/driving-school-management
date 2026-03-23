namespace driving_school_management.ViewModels
{
    public class HocAllViewModel
    {
        public string SelectedHang { get; set; } = string.Empty;
        public bool IsXeMay { get; set; }
        public int TotalChapters { get; set; }
        public int TotalQuestions { get; set; }
        public List<HocAllChapterVM> Chapters { get; set; } = new();
    }
}