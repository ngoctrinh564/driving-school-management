using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Tinhhuongmophong
{
    public decimal Tinhhuongmophongid { get; set; }

    public decimal Chuongmophongid { get; set; }

    public string? Tieude { get; set; }

    public string? Videourl { get; set; }

    public decimal? Thutu { get; set; }

    public bool? Kho { get; set; }

    public float? Tgbatdau { get; set; }

    public float? Tgketthuc { get; set; }

    public DateTime Ngaytao { get; set; }

    public string? Urlanhmeo { get; set; }

    public virtual ICollection<Chitietbodemophong> Chitietbodemophongs { get; set; } = new List<Chitietbodemophong>();

    public virtual Chuongmophong Chuongmophong { get; set; } = null!;

    public virtual ICollection<Diemtungtinhhuong> Diemtungtinhhuongs { get; set; } = new List<Diemtungtinhhuong>();
}
