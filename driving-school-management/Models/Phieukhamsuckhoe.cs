using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Phieukhamsuckhoe
{
    public decimal Khamsuckhoeid { get; set; }

    public string? Hieuluc { get; set; }

    public DateTime? Thoihan { get; set; }

    public string? Khammat { get; set; }

    public string? Huyetap { get; set; }

    public decimal? Chieucao { get; set; }

    public decimal? Cannang { get; set; }

    public string? Urlanh { get; set; }

    public virtual ICollection<Anhgksk> Anhgksks { get; set; } = new List<Anhgksk>();

    public virtual ICollection<Hosothisinh> Hosothisinhs { get; set; } = new List<Hosothisinh>();
}
