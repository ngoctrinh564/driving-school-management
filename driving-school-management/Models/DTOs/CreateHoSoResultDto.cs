namespace driving_school_management.Models.DTOs
{
    public class CreateHoSoResultDto
    {
        public bool Success { get; set; }
        public int? HoSoId { get; set; }
        public int? KhamSucKhoeId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
