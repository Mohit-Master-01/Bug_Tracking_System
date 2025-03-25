using System;
using System.Collections.Generic;

namespace Bug_Tracking_System.Models;

public partial class UserProject
{
    public int UserProjectId { get; set; }

    public int? UserId { get; set; }

    public int? ProjectId { get; set; }

    public virtual Project? Project { get; set; }

    public virtual User? User { get; set; }
}
