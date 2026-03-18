using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Kythi
{
    public decimal Kythiid { get; set; }

    public string Tenkythi { get; set; } = null!;

    public string? Loaikythi { get; set; }

    public virtual ICollection<Baithi> Baithis { get; set; } = new List<Baithi>();

    public virtual ICollection<Chitietdangkythi> Chitietdangkythis { get; set; } = new List<Chitietdangkythi>();

    public virtual ICollection<Lichthi> Lichthis { get; set; } = new List<Lichthi>();
}
