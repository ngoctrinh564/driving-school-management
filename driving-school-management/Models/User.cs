using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class User
{
    public decimal Userid { get; set; }

    public decimal Roleid { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool? Isactive { get; set; }

    public virtual ICollection<Bailammophong> Bailammophongs { get; set; } = new List<Bailammophong>();

    public virtual ICollection<Bailam> Bailams { get; set; } = new List<Bailam>();

    public virtual ICollection<Hocvien> Hocviens { get; set; } = new List<Hocvien>();

    public virtual Role Role { get; set; } = null!;
}
