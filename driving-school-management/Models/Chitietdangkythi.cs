using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Chitietdangkythi
{
    public decimal Kythiid { get; set; }

    public decimal Hosoid { get; set; }

    public DateTime? Thoigiandangky { get; set; }

    public virtual Hosothisinh Hoso { get; set; } = null!;

    public virtual Kythi Kythi { get; set; } = null!;
}
