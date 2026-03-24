namespace driving_school_management.Models.DTOs
{
    public class OracleExecResultDto
    {
        public int TongDiem { get; set; }
        public bool Dat { get; set; }
        public int? IdBaiLam { get; set; }
    }

    public class RandomKetQuaRequest
    {
        public List<int> SelectedThIds { get; set; } = new();
        public List<FlagItem> Flags { get; set; } = new();
    }
}
