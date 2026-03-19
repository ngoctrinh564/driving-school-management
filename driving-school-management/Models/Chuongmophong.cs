using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Chuongmophong
{
    public decimal Chuongmophongid { get; set; }

    public string Tenchuong { get; set; } = null!;

    public decimal? Thutu { get; set; }

    public virtual ICollection<Tinhhuongmophong> Tinhhuongmophongs { get; set; } = new List<Tinhhuongmophong>();
}
