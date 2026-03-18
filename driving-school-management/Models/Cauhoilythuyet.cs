using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Cauhoilythuyet
{
    public decimal Cauhoiid { get; set; }

    public decimal Chuongid { get; set; }

    public string? Noidung { get; set; }

    public string? Hinhanh { get; set; }

    public bool? Cauliet { get; set; }

    public bool? Chuy { get; set; }

    public bool? Xemay { get; set; }

    public string? Urlanhmeo { get; set; }

    public virtual ICollection<Chitietbailam> Chitietbailams { get; set; } = new List<Chitietbailam>();

    public virtual ICollection<Chitietbodetracnghiem> Chitietbodetracnghiems { get; set; } = new List<Chitietbodetracnghiem>();

    public virtual Chuong Chuong { get; set; } = null!;

    public virtual ICollection<Dapan> Dapans { get; set; } = new List<Dapan>();
}
