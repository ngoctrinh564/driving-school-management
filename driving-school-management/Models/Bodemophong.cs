using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Bodemophong
{
    public decimal Bodemophongid { get; set; }

    public string? Tenbode { get; set; }

    public decimal? Sotinhhuong { get; set; }

    public DateTime Taoluc { get; set; }

    public bool? Isactive { get; set; }

    public virtual ICollection<Bailammophong> Bailammophongs { get; set; } = new List<Bailammophong>();

    public virtual ICollection<Chitietbodemophong> Chitietbodemophongs { get; set; } = new List<Chitietbodemophong>();
}
