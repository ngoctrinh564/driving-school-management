using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Chitietphieuthanhtoan
{
    public decimal Hosoid { get; set; }

    public decimal Phieuid { get; set; }

    public string? Loaiphi { get; set; }

    public string? Ghichu { get; set; }

    public virtual Hosothisinh Hoso { get; set; } = null!;

    public virtual Phieuthanhtoan Phieu { get; set; } = null!;
}
