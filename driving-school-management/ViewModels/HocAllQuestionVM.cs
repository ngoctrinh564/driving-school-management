namespace driving_school_management.ViewModels
{
    public class HocAllQuestionVM
    {
        public int GlobalIndex { get; set; }
        public int IdCauHoi { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? UrlAnhMeo { get; set; }
        public bool IsCauLiet { get; set; }
        public bool IsChuY { get; set; }
        public bool IsXeMay { get; set; }
        public List<HocAllAnswerVM> DapAns { get; set; } = new();
    }
}