using System;
using System.Collections.Generic;

namespace Bug_Tracking_System.Models;

public partial class Attachment
{
    public int AttachmentId { get; set; }

    public int BugId { get; set; }

    public string FilePath { get; set; } = null!;

    public DateTime? UploadedDate { get; set; }

    public virtual Bug Bug { get; set; } = null!;
}
