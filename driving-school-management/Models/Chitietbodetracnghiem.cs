using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Chitietbodetracnghiem
{
    public decimal Bodeid { get; set; }

    public decimal Cauhoiid { get; set; }

    public decimal Thutu { get; set; }

    public virtual Bodethithu Bode { get; set; } = null!;

    public virtual Cauhoilythuyet Cauhoi { get; set; } = null!;
}
