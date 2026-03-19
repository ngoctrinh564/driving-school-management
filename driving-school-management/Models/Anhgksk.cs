using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Anhgksk
{
    public decimal Anhid { get; set; }

    public decimal Khamsuckhoeid { get; set; }

    public string Urlanh { get; set; } = null!;

    public virtual Phieukhamsuckhoe Khamsuckhoe { get; set; } = null!;
}
