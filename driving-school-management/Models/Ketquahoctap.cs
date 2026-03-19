using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Ketquahoctap
{
    public decimal Ketquahoctapid { get; set; }

    public decimal Hosoid { get; set; }

    public string? Nhanxet { get; set; }

    public decimal? Sobuoihoc { get; set; }

    public decimal? Sobuoivang { get; set; }

    public string? Sokmhoanthanh { get; set; }

    public virtual Chitietketquahoctap? Chitietketquahoctap { get; set; }

    public virtual Hosothisinh Hoso { get; set; } = null!;
}
