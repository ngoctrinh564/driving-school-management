using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Chitietketquathi
{
    public decimal Baithiid { get; set; }

    public decimal Hosoid { get; set; }

    public string? Ketquadatduoc { get; set; }

    public float? Tongdiem { get; set; }

    public virtual Baithi Baithi { get; set; } = null!;

    public virtual Hosothisinh Hoso { get; set; } = null!;
}
