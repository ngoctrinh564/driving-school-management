namespace driving_school_management.ViewModels
{
    public class ExamQuestionVM
    {
        public int IdCauHoi { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public bool LaCauLiet { get; set; }
        public string? ImageUrl { get; set; }
        public string? UrlAnhMeo { get; set; }
        public List<ExamAnswerVM> DapAn { get; set; } = new();
    }
}