using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Khoahoc
{
    public decimal Khoahocid { get; set; }

    public decimal Hangid { get; set; }

    public string? Tenkhoahoc { get; set; }

    public DateTime? Ngaybatdau { get; set; }

    public DateTime? Ngayketthuc { get; set; }

    public string? Diadiem { get; set; }

    public string? Trangthai { get; set; }

    public virtual ICollection<Chitietketquahoctap> Chitietketquahoctaps { get; set; } = new List<Chitietketquahoctap>();

    public virtual Hanggplx Hang { get; set; } = null!;
}
