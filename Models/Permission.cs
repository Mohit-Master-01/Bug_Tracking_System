using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bug_Tracking_System.Models;

public partial class Permission
{
    public int Permissionid { get; set; }

    public int Roleid { get; set; }

    public int Tabid { get; set; }

    [NotMapped]
    public string TabName { get; set; }

    public bool Isactive { get; set; }

    public string? PermissionType { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual TblTab Tab { get; set; } = null!;
}
