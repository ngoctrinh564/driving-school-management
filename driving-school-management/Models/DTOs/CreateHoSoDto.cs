namespace driving_school_management.Models.DTOs
{
    public class CreateHoSoDto
    {
        public int HangId { get; set; }
        public string LoaiHoSo { get; set; }
        public string? GhiChu { get; set; }

        public string? HieuLuc { get; set; }
        public DateTime? ThoiHan { get; set; }
        public string? KhamMat { get; set; }
        public string? HuyetAp { get; set; }
        public decimal? ChieuCao { get; set; }
        public decimal? CanNang { get; set; }

        public List<IFormFile>? AnhGkskFiles { get; set; }
    }
}
