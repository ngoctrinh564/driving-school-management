using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Phieuthanhtoan
{
    public decimal Phieuid { get; set; }

    public string Tenphieu { get; set; } = null!;

    public DateTime? Ngaylap { get; set; }

    public decimal? Tongtien { get; set; }

    public DateTime? Ngaynop { get; set; }

    public string? Phuongthuc { get; set; }

    public virtual ICollection<Chitietphieuthanhtoan> Chitietphieuthanhtoans { get; set; } = new List<Chitietphieuthanhtoan>();
}
