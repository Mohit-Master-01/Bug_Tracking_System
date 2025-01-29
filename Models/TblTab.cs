using System;
using System.Collections.Generic;

namespace Bug_Tracking_System.Models;

public partial class TblTab
{
    public int TabId { get; set; }

    public string TabName { get; set; } = null!;

    public string TabUrl { get; set; } = null!;

    public int? ParentId { get; set; }

    public string? IconPath { get; set; }

    public bool? IsActive { get; set; }

    public int? SortOrder { get; set; }

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
