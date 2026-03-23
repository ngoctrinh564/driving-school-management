namespace driving_school_management.ViewModels
{
    public class ExamAnswerVM
    {
        public int IdDapAn { get; set; }
        public string Label { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}