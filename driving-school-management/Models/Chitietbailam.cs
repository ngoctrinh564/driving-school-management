using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Chitietbailam
{
    public decimal Bailamid { get; set; }

    public decimal Cauhoiid { get; set; }

    public string? Dapandachon { get; set; }

    public bool? Ketquacau { get; set; }

    public virtual Bailam Bailam { get; set; } = null!;

    public virtual Cauhoilythuyet Cauhoi { get; set; } = null!;
}
