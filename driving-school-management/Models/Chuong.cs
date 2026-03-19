using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Chuong
{
    public decimal Chuongid { get; set; }

    public string Tenchuong { get; set; } = null!;

    public decimal? Thutu { get; set; }

    public virtual ICollection<Cauhoilythuyet> Cauhoilythuyets { get; set; } = new List<Cauhoilythuyet>();
}
