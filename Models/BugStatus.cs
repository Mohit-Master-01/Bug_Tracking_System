using System;
using System.Collections.Generic;

namespace Bug_Tracking_System.Models;

public partial class BugStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Bug> Bugs { get; set; } = new List<Bug>();
}
