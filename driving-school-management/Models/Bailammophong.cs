using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Bailammophong
{
    public decimal Bailammophongid { get; set; }

    public decimal Tongdiem { get; set; }

    public bool? Ketqua { get; set; }

    public decimal Userid { get; set; }

    public decimal Bodemophongid { get; set; }

    public virtual Bodemophong Bodemophong { get; set; } = null!;

    public virtual ICollection<Diemtungtinhhuong> Diemtungtinhhuongs { get; set; } = new List<Diemtungtinhhuong>();

    public virtual User User { get; set; } = null!;
}
