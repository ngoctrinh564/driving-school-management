namespace driving_school_management.Models.DTOs
{
    public class HoSoIndexStatusDto
    {
        public int TongHoSo { get; set; }
        public int TongHoSoConHan { get; set; }
        public int TongHoSoDaDuyetConHan { get; set; }
        public int TongHoSoDangXuLyConHan { get; set; }
        public int TongHoSoHetHan { get; set; }
        public int ShowModal { get; set; }
        public string StatusCode { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
    }
}