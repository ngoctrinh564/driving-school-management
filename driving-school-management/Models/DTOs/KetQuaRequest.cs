namespace driving_school_management.Models.DTOs
{
    public class KetQuaRequest
    {
        public int IdBoDe { get; set; }
        public List<FlagItem> Flags { get; set; } = new();
    }
}
