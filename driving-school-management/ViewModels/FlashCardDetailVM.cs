using driving_school_management.ViewModels;
using System.Collections.Generic;

namespace driving_school_management.ViewModels
{
    public class FlashCardDetailVM
    {
        public int IdBienBao { get; set; }
        public string TenBienBao { get; set; }
        public string Ynghia { get; set; }
        public string HinhAnh { get; set; }
        public List<FlashCardItemVM> Items { get; set; } = new List<FlashCardItemVM>();
    }
}