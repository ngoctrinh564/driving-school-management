using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Chitietketquahoctap
{
    public decimal Ketquahoctapid { get; set; }

    public decimal Khoahocid { get; set; }

    public bool? Lythuyetkq { get; set; }

    public bool? Sahinhkq { get; set; }

    public bool? Duongtruongkq { get; set; }

    public bool? Mophongkq { get; set; }

    public virtual Ketquahoctap Ketquahoctap { get; set; } = null!;

    public virtual Khoahoc Khoahoc { get; set; } = null!;
}
