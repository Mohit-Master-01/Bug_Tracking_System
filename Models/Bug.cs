using System;
using System.Collections.Generic;

namespace Bug_Tracking_System.Models;

public partial class Bug
{
    public int BugId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Severity { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public int StatusId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? ProjectId { get; set; }

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Project? Project { get; set; }

    public virtual BugStatus Status { get; set; } = null!;

    public virtual ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
