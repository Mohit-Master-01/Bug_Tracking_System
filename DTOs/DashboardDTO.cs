namespace Bug_Tracking_System.DTOs
{
    public class DashboardDTO
    {
        public int Role { get; set; }


        // Dynamic Count Section
        public int TotalUsers { get; set; }
        public int TotalProjectManagers { get; set; }
        public int TotalDevelopers { get; set; }
        public int TotalTesters { get; set; }
        public int TotalProjects { get; set; }
        public int TotalBugs { get; set; }

        // New
        public int? TotalBugsInCurrentProject { get; set; }
        public int? DevelopersInCurrentProject { get; set; }
        public int? ProjectProgress { get; set; }

        public int MyProjects { get; set; }
        public int BugsInMyProjects { get; set; }
        public int MyBugs { get; set; }
        public int ResolvedBugs { get; set; }
        public int InProgressBugs { get; set; }
        public int BugsToVerify { get; set; }
        public int VerifiedBugs { get; set; }
        public int TotalBugsTested { get; set; }

        // Graph Data
        public Dictionary<string, int> UsersByRole { get; set; }
        public Dictionary<string, int> BugsByStatus { get; set; }
        public Dictionary<string, int> BugsBySeverity { get; set; }
        public Dictionary<string, int> BugsByProject { get; set; }
        public Dictionary<string, int> BugsOverTime { get; set; }
        public Dictionary<string, Dictionary<string, int>> ProjectActivity { get; set; }

        // Custom Graphs for PM, Dev, Tester
        public Dictionary<string, int> MyBugsByStatus { get; set; }
        public Dictionary<string, int> MyBugsBySeverity { get; set; }
        public Dictionary<string, int> BugActivityTimeline { get; set; }
        public Dictionary<string, int> BugVerificationHistory { get; set; }
        public Dictionary<string, int> SeverityOfBugsTested { get; set; }
    }
}
