using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Canbogiamsat
{
    public decimal Canboid { get; set; }

    public string Hoten { get; set; } = null!;

    public DateTime? Ngaysinh { get; set; }

    public string? Gioitinh { get; set; }

    public string? Diachi { get; set; }

    public string? Email { get; set; }

    public string? Sdt { get; set; }

    public virtual ICollection<Chitietphanconggiamsat> Chitietphanconggiamsats { get; set; } = new List<Chitietphanconggiamsat>();
}
