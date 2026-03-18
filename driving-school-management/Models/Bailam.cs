using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Bailam
{
    public decimal Bailamid { get; set; }

    public decimal? Thoigianlambai { get; set; }

    public decimal Socausai { get; set; }

    public bool? Ketqua { get; set; }

    public decimal Userid { get; set; }

    public decimal Bodeid { get; set; }

    public virtual Bodethithu Bode { get; set; } = null!;

    public virtual ICollection<Chitietbailam> Chitietbailams { get; set; } = new List<Chitietbailam>();

    public virtual User User { get; set; } = null!;
}
