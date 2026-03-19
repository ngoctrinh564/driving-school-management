using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Baithi
{
    public decimal Baithiid { get; set; }

    public string Tenbaithi { get; set; } = null!;

    public string? Mota { get; set; }

    public string? Loaibaithi { get; set; }

    public decimal Kythiid { get; set; }

    public virtual ICollection<Chitietketquathi> Chitietketquathis { get; set; } = new List<Chitietketquathi>();

    public virtual ICollection<Chitietphanconggiamsat> Chitietphanconggiamsats { get; set; } = new List<Chitietphanconggiamsat>();

    public virtual Kythi Kythi { get; set; } = null!;
}
