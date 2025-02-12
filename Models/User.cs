using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bug_Tracking_System.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    [NotMapped]
    public string? ConfirmPassword {  get; set; }

    public int? RoleId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool? IsActive { get; set; }

    public string? Otp { get; set; }

    public DateTime? OtpExpiry { get; set; }

    public bool? IsEmailVerified { get; set; }

    public string? ProfileImage { get; set; }

    public bool? IsAdmin { get; set; }

    public int? ProjectId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? LinkedInProfile { get; set; }

    public string? GitHubProfile { get; set; }

    public string? Bio { get; set; }

    public string? Skills { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Bug> Bugs { get; set; } = new List<Bug>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [NotMapped]
    public virtual Project? Project { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    [NotMapped]
    public virtual Role? Role { get; set; }

    public virtual ICollection<TaskAssignment> TaskAssignmentAssignedByNavigations { get; set; } = new List<TaskAssignment>();

    public virtual ICollection<TaskAssignment> TaskAssignmentAssignedToNavigations { get; set; } = new List<TaskAssignment>();

    public virtual ICollection<TaskAssignment> TaskAssignmentProjectManagers { get; set; } = new List<TaskAssignment>();
}
