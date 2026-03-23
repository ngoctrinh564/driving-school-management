namespace driving_school_management.Models.DTOs
{
    public class KhoaHocDetailDto : KhoaHocDto
    {
        public int HangId { get; set; }
        public string MoTa { get; set; }
        public string LoaiPhuongTien { get; set; }
        public int SoCauHoi { get; set; }
        public int DiemDat { get; set; }
        public int ThoiGianTn { get; set; }
    }
}
