using System;
using System.Collections.Generic;

namespace driving_school_management.Models;

public partial class Role
{
    public decimal Roleid { get; set; }

    public string Rolename { get; set; } = null!;

    public string? Mota { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
