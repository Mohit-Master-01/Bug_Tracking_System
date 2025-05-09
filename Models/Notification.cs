﻿using System;
using System.Collections.Generic;

namespace Bug_Tracking_System.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime NotificationDate { get; set; }

    public int? Type { get; set; }

    public int? RelatedId { get; set; }

    public string? ModuleType { get; set; }

    public virtual User User { get; set; } = null!;
}
