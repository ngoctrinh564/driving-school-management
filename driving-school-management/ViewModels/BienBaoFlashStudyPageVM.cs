namespace driving_school_management.ViewModels
{
    public class BienBaoFlashStudyPageVM
    {
        public bool IsLoggedIn { get; set; }
        public string LoginUrl { get; set; } = "";

        public List<BienBaoFlashCardVM> Cards { get; set; } = new();
    }
}