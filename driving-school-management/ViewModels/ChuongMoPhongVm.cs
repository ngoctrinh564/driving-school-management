namespace driving_school_management.ViewModels
{
    public class ChuongMoPhongVm
    {
        public int IdChuongMp { get; set; }
        public string TenChuong { get; set; } = string.Empty;
        public List<TinhHuongItem2> TinhHuongs { get; set; } = new();
    }
}