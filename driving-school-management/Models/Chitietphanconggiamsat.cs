using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Chitietphanconggiamsat
{
    public decimal Baithiid { get; set; }

    public decimal Canboid { get; set; }

    public DateTime? Thoigianbatdau { get; set; }

    public DateTime? Thoigianketthuc { get; set; }

    public string? Phongthi { get; set; }

    public string? Ghichu { get; set; }

    public virtual Baithi Baithi { get; set; } = null!;

    public virtual Canbogiamsat Canbo { get; set; } = null!;
}
