using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Chitietgplx
{
    public decimal Hangid { get; set; }

    public decimal Gplxid { get; set; }

    public DateTime? Ngaycapchitiet { get; set; }

    public virtual Giaypheplaixe Gplx { get; set; } = null!;

    public virtual Hanggplx Hang { get; set; } = null!;
}
