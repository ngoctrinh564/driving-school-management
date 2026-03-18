using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Chitietbodemophong
{
    public decimal Bodemophongid { get; set; }

    public decimal Tinhhuongmophongid { get; set; }

    public decimal? Thutu { get; set; }

    public virtual Bodemophong Bodemophong { get; set; } = null!;

    public virtual Tinhhuongmophong Tinhhuongmophong { get; set; } = null!;
}
