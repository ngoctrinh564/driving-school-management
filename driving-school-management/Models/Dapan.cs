using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Dapan
{
    public decimal Dapanid { get; set; }

    public decimal Cauhoiid { get; set; }

    public string? Noidung { get; set; }

    public bool? Dapandung { get; set; }

    public decimal Thutu { get; set; }

    public virtual Cauhoilythuyet Cauhoi { get; set; } = null!;
}
