using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Lichthi
{
    public decimal Lichthiid { get; set; }

    public DateTime? Thoigianthi { get; set; }

    public string? Diadiem { get; set; }

    public decimal Kythiid { get; set; }

    public virtual Kythi Kythi { get; set; } = null!;
}
