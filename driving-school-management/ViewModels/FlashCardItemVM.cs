namespace driving_school_management.ViewModels
{
    public class FlashCardItemVM
    {
        public int IdFlashcard { get; set; }
        public string DanhGia { get; set; } = "";
        public int UserId { get; set; }
        public string HoTen { get; set; } = "";

        public string TrangThaiGhiNho
        {
            get
            {
                var value = DanhGia?.Trim().ToLower() ?? "";
                return value == "nho" ? "Đã nhớ" : "Chưa nhớ";
            }
        }
    }
}