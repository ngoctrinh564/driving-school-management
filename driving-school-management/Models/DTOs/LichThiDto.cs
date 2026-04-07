namespace driving_school_management.Models.DTOs
{
    public class LichThiDto
    {
        public int LichThiId { get; set; }
        public int KyThiId { get; set; }
        public string? DiaDiem { get; set; }
        public DateTime? ThoiGianThi { get; set; }
    }
}