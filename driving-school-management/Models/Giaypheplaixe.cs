using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Giaypheplaixe
{
    public decimal Gplxid { get; set; }

    public DateTime? Ngaycap { get; set; }

    public DateTime? Ngayhethan { get; set; }

    public string? Trangthai { get; set; }

    public decimal Hosoid { get; set; }

    public virtual ICollection<Chitietgplx> Chitietgplxes { get; set; } = new List<Chitietgplx>();

    public virtual Hosothisinh Hoso { get; set; } = null!;

    public virtual ICollection<Yeucaunanghang> Yeucaunanghangs { get; set; } = new List<Yeucaunanghang>();
}
