using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Hocvien
{
    public decimal Hocvienid { get; set; }

    public string Hoten { get; set; } = null!;

    public string Socmndcccd { get; set; } = null!;

    public DateTime? Namsinh { get; set; }

    public string? Gioitinh { get; set; }

    public string? Sdt { get; set; }

    public string? Email { get; set; }

    public string? Avatarurl { get; set; }

    public decimal? Userid { get; set; }

    public virtual ICollection<Hosothisinh> Hosothisinhs { get; set; } = new List<Hosothisinh>();

    public virtual User? User { get; set; }
}
