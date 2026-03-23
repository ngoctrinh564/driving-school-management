namespace driving_school_management.ViewModels
{
    public class BienBaoFlashCardVM
    {
        public int IdBienBao { get; set; }
        public string TenBienBao { get; set; } = "";
        public string? YNghia { get; set; }
        public string? HinhAnh { get; set; }

        public int? IdFlashcard { get; set; }
        public string? DanhGia { get; set; }
    }
}