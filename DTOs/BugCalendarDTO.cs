namespace Bug_Tracking_System.DTOs
{
    public class BugCalendarDTO
    {
        public int BugId { get; set; }           // For reference
        public string Title { get; set; }        // Event title
        public string Start { get; set; }        // Event start date in yyyy-MM-dd format
        public string Url { get; set; }          // Link to bug details
        public string Severity { get; set; }     // Optional styling/filter
        public int? ProjectId { get; set; }      // For filtering by project
        public string Status { get; set; }       // Optional styling/filter
    }
}
