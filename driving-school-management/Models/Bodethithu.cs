using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Bodethithu
{
    public decimal Bodeid { get; set; }

    public string? Tenbode { get; set; }

    public decimal? Thoigian { get; set; }

    public decimal? Socauhoi { get; set; }

    public bool? Hoatdong { get; set; }

    public DateTime Taoluc { get; set; }

    public decimal Hangid { get; set; }

    public virtual ICollection<Bailam> Bailams { get; set; } = new List<Bailam>();

    public virtual ICollection<Chitietbodetracnghiem> Chitietbodetracnghiems { get; set; } = new List<Chitietbodetracnghiem>();

    public virtual Hanggplx Hang { get; set; } = null!;
}
