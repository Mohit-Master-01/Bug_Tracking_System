using System;
using System.Collections.Generic;

namespace Bug_Tracking_System.Models;

public partial class AuditLog
{
    public int LogId { get; set; }

    public int UserId { get; set; }

    public string Action { get; set; } = null!;

    public DateTime? ActionDate { get; set; }

    public virtual User User { get; set; } = null!;
}
