using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Yeucaunanghang
{
    public decimal Yeucauid { get; set; }

    public string? Noidung { get; set; }

    public string? Dieukien { get; set; }

    public decimal Gplxid { get; set; }

    public virtual Giaypheplaixe Gplx { get; set; } = null!;
}
