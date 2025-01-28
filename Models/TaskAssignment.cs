using System;
using System.Collections.Generic;

namespace Bug_Tracking_System.Models;

public partial class TaskAssignment
{
    public int TaskId { get; set; }

    public int BugId { get; set; }

    public int AssignedTo { get; set; }

    public int AssignedBy { get; set; }

    public int ProjectManagerId { get; set; }

    public DateTime? AssignedDate { get; set; }

    public DateTime? CompletionDate { get; set; }

    public int? ProjectId { get; set; }

    public virtual User AssignedByNavigation { get; set; } = null!;

    public virtual User AssignedToNavigation { get; set; } = null!;

    public virtual Bug Bug { get; set; } = null!;

    public virtual Project? Project { get; set; }

    public virtual User ProjectManager { get; set; } = null!;
}
