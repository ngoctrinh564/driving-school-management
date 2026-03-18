using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Diemtungtinhhuong
{
    public decimal Bailammophongid { get; set; }

    public decimal Tinhhuongmophongid { get; set; }

    public float Thoidiemnguoidungnhan { get; set; }

    public virtual Bailammophong Bailammophong { get; set; } = null!;

    public virtual Tinhhuongmophong Tinhhuongmophong { get; set; } = null!;
}
