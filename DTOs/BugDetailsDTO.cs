using Bug_Tracking_System.Models;

namespace Bug_Tracking_System.DTOs
{
    public class BugDetailsDTO
    {
        public Bug Bug { get; set; }
        public string? AssignedToName { get; set; }
        public string? ProjectManagerName { get; set; }
        public DateTime? CompletionDate { get; set; }
    }
}
