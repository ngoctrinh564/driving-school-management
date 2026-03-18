using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Hanggplx
{
    public decimal Hangid { get; set; }

    public string Mahang { get; set; } = null!;

    public string Tenhang { get; set; } = null!;

    public string? Mota { get; set; }

    public string? Loaiphuongtien { get; set; }

    public decimal Socauhoi { get; set; }

    public decimal Diemdat { get; set; }

    public decimal Thoigiantn { get; set; }

    public decimal? Thoihanlythuyet { get; set; }

    public decimal? Thoihanthuchanh { get; set; }

    public decimal? Hocphi { get; set; }

    public virtual ICollection<Bodethithu> Bodethithus { get; set; } = new List<Bodethithu>();

    public virtual ICollection<Chitietgplx> Chitietgplxes { get; set; } = new List<Chitietgplx>();

    public virtual ICollection<Hosothisinh> Hosothisinhs { get; set; } = new List<Hosothisinh>();

    public virtual ICollection<Khoahoc> Khoahocs { get; set; } = new List<Khoahoc>();
}
