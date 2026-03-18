using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Hosothisinh
{
    public decimal Hosoid { get; set; }

    public decimal Hocvienid { get; set; }

    public string Tenhoso { get; set; } = null!;

    public string? Loaihoso { get; set; }

    public DateTime? Ngaydangky { get; set; }

    public string? Trangthai { get; set; }

    public string? Ghichu { get; set; }

    public decimal? Khamsuckhoeid { get; set; }

    public decimal Hangid { get; set; }

    public virtual ICollection<Chitietdangkythi> Chitietdangkythis { get; set; } = new List<Chitietdangkythi>();

    public virtual ICollection<Chitietketquathi> Chitietketquathis { get; set; } = new List<Chitietketquathi>();

    public virtual ICollection<Chitietphieuthanhtoan> Chitietphieuthanhtoans { get; set; } = new List<Chitietphieuthanhtoan>();

    public virtual ICollection<Giaypheplaixe> Giaypheplaixes { get; set; } = new List<Giaypheplaixe>();

    public virtual Hanggplx Hang { get; set; } = null!;

    public virtual Hocvien Hocvien { get; set; } = null!;

    public virtual ICollection<Ketquahoctap> Ketquahoctaps { get; set; } = new List<Ketquahoctap>();

    public virtual Phieukhamsuckhoe? Khamsuckhoe { get; set; }
}
