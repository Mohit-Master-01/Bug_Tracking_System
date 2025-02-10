using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bug_Tracking_System.Models;

public partial class Project
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = null!;

    public string? Description { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool? IsActive { get; set; }

    public string? Status { get; set; }

    public int? Completion { get; set; }

    public virtual ICollection<Bug> Bugs { get; set; } = new List<Bug>();

    [JsonIgnore]
    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
