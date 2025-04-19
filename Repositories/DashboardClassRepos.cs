using Bug_Tracking_System.DTOs;
using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bug_Tracking_System.Repositories
{
    public class DashboardClassRepos : IDashboardRepos
    {
        private readonly DbBug _dbBug;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public DashboardClassRepos(DbBug dbBug, IHttpContextAccessor httpContextAccessor)
        {
            _dbBug = dbBug;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<DashboardDTO> GetDashboardData(int userId, int role)
        {
            int? currentProjectId = _httpContextAccessor.HttpContext?.Session.GetInt32("CurrentProjectId");

            // Get all project IDs assigned to the current PM
            var myProjectIds = await _dbBug.UserProjects
                .Where(t => t.UserId == userId)
                .Select(t => t.ProjectId)
                .Distinct()
                .ToListAsync();

            var dto = new DashboardDTO
            {
                Role = role
            };

            switch (role)
            {
                case 4:
                    dto.TotalUsers = await _dbBug.Users.Where(u => u.RoleId != 4 && u.IsActive == true).CountAsync();
                    dto.TotalProjectManagers = await _dbBug.Users.CountAsync(u => u.Role.RoleId == 1 && u.IsActive == true);
                    dto.TotalDevelopers = await _dbBug.Users.CountAsync(u => u.Role.RoleId == 2 && u.IsActive == true);
                    dto.TotalTesters = await _dbBug.Users.CountAsync(u => u.Role.RoleId == 3 && u.IsActive == true);
                    dto.TotalProjects = await _dbBug.Projects.CountAsync(p => p.IsActive == true);
                    dto.TotalBugs = await _dbBug.Bugs.CountAsync(b => b.IsActive == true);

                    dto.UsersByRole = await _dbBug.Users
                        .Where(u => u.RoleId !=4 && u.IsActive == true)
                        .GroupBy(u => u.Role.RoleName)
                        .ToDictionaryAsync(g => g.Key, g => g.Count());

                    dto.BugsByStatus = await _dbBug.Bugs
                        .GroupBy(b => b.Status.StatusName)
                        .ToDictionaryAsync(g => g.Key, g => g.Count());

                    dto.BugsBySeverity = await _dbBug.Bugs
                        .GroupBy(b => b.Severity)
                        .ToDictionaryAsync(g => g.Key, g => g.Count());

                    dto.BugsByProject = await _dbBug.Bugs
                        .Where(b => b.ProjectId != null)
                        .GroupBy(b => b.Project.ProjectName)
                        .ToDictionaryAsync(g => g.Key, g => g.Count());

                    dto.BugsOverTime = await _dbBug.Bugs
                        .GroupBy(b => b.CreatedDate.Date)
                        .ToDictionaryAsync(g => g.Key.ToShortDateString(), g => g.Count());
                    break;

                case 1:


                    

                    // Always show total projects and bugs under this PM
                    dto.MyProjects = myProjectIds.Count;
                    dto.BugsInMyProjects = await _dbBug.Bugs
                        .CountAsync(b => b.ProjectId != null && myProjectIds.Contains(b.ProjectId.Value));

                    // Show project-wise data only when a project is selected
                    if (currentProjectId != null)
                    {
                        int projectId = currentProjectId.Value;

                        // Total Bugs in Current Project
                        dto.TotalBugsInCurrentProject = await _dbBug.Bugs
                            .CountAsync(b => b.ProjectId == projectId);

                        // Developers in Current Project
                        dto.DevelopersInCurrentProject = await _dbBug.UserProjects
                            .Where(up => up.ProjectId == projectId && up.User.Role.RoleName == "Developer")
                            .Select(up => up.UserId)
                            .Distinct()
                            .CountAsync();

                        // Bugs by Status
                        dto.MyBugsByStatus = await _dbBug.Bugs
                            .Where(b => b.ProjectId == projectId)
                            .GroupBy(b => b.Status.StatusName)
                            .ToDictionaryAsync(g => g.Key, g => g.Count());

                        // Bugs by Severity
                        dto.MyBugsBySeverity = await _dbBug.Bugs
                            .Where(b => b.ProjectId == projectId)
                            .GroupBy(b => b.Severity)
                            .ToDictionaryAsync(g => g.Key, g => g.Count());

                        // Bug Activity Timeline (Created Over Time)
                        dto.BugActivityTimeline = await _dbBug.Bugs
                            .Where(b => b.ProjectId == projectId)
                            .GroupBy(b => b.CreatedDate.Date)
                            .ToDictionaryAsync(g => g.Key.ToShortDateString(), g => g.Count());

                        // Project Progress (based on completion field)
                        var project = await _dbBug.Projects.FirstOrDefaultAsync(p => p.ProjectId == projectId);
                        dto.ProjectProgress = project?.Completion ?? 0;
                    }

                    // Total Testers (across all assigned projects)
                    dto.TotalTesters = await _dbBug.Users.CountAsync(u => u.Role.RoleId == 3);

                    //dto.TotalTesters = await _dbBug.UserProjects
                    //    .Where(up => myProjectIds.Contains(up.ProjectId) && up.User.Role.RoleName == "Tester")
                    //    .Select(up => up.UserId)
                    //    .Distinct()
                    //    .CountAsync();
                    break;


                case 2:



                    var allAssignments = _dbBug.TaskAssignments
                   .Where(t => t.AssignedTo == userId);

                    // Filter for selected project (if any)
                    var scopedAssignments = currentProjectId.HasValue
                        ? allAssignments.Where(t => t.ProjectId == currentProjectId.Value)
                        : allAssignments;



                    // Distinct bug and project IDs in scope
                    var scopedBugIds = await scopedAssignments.Select(t => t.BugId).Distinct().ToListAsync();
                    var scopedProjectIds = await scopedAssignments.Select(t => t.ProjectId).Distinct().ToListAsync();

                    // Assignments stats
                    dto.MyProjects = myProjectIds.Count;
                    dto.MyBugs = await _dbBug.Bugs
                                            .CountAsync(b => b.ProjectId != null && myProjectIds.Contains(b.ProjectId.Value));

                    // Resolved & In-Progress bugs in current scope
                    dto.ResolvedBugs = await _dbBug.Bugs
                        .CountAsync(b => b.ProjectId != null && myProjectIds.Contains(b.ProjectId.Value) && b.Status.StatusId == 3);

                    dto.InProgressBugs = await _dbBug.Bugs
                        .CountAsync(b => b.ProjectId != null && myProjectIds.Contains(b.ProjectId.Value) && b.Status.StatusId != 3);

                    // Charts based on scoped bugs
                    dto.MyBugsByStatus = await _dbBug.Bugs
                        .Where(b => scopedBugIds.Contains(b.BugId))
                        .GroupBy(b => b.Status.StatusName)
                        .ToDictionaryAsync(g => g.Key, g => g.Count());

                    dto.MyBugsBySeverity = await _dbBug.Bugs
                        .Where(b => scopedBugIds.Contains(b.BugId))
                        .GroupBy(b => b.Severity)
                        .ToDictionaryAsync(g => g.Key, g => g.Count());

                    // Filter for selected project (if any)
                    var projectScopedTasks = currentProjectId.HasValue
                        ? allAssignments.Where(t => t.ProjectId == currentProjectId.Value)
                        : allAssignments;

                    var projectScopedBugIds = await projectScopedTasks
                    .Select(t => t.BugId)
                    .Distinct()
                    .ToListAsync();

                    dto.BugActivityTimeline = await _dbBug.Bugs
                        .Where(b => projectScopedBugIds.Contains(b.BugId))
                        .GroupBy(b => b.CreatedDate.Date)
                        .ToDictionaryAsync(g => g.Key.ToShortDateString(), g => g.Count());

                    // Set fallback for chart bindings
                    dto.BugsByStatus = dto.MyBugsByStatus;
                    dto.BugsBySeverity = dto.MyBugsBySeverity;
                    break;


                case 3:
                    // Step 1: Get all bugs assigned to the current tester
                    var testerBugIds = await _dbBug.Bugs
                        .Select(t => t.BugId)
                        .Distinct()
                        .ToListAsync();

                    // Step 2: Bug Stats
                    dto.TotalBugsTested = testerBugIds.Count;

                    dto.VerifiedBugs = await _dbBug.Bugs
                        .CountAsync(b => testerBugIds.Contains(b.BugId) && b.Status.StatusId == 1002);

                    dto.BugsToVerify = await _dbBug.Bugs
                        .CountAsync(b => testerBugIds.Contains(b.BugId) && b.Status.StatusId == 3);

                    // Step 3: Bug Verification History (Line Chart — optional: use CreatedDate.Date group)
                    dto.BugVerificationHistory = await _dbBug.Bugs
                        .Where(b => testerBugIds.Contains(b.BugId))
                        .GroupBy(b => b.CreatedDate.Date)
                        .ToDictionaryAsync(g => g.Key.ToShortDateString(), g => g.Count());

                    // Step 4: Bugs by Status (Pie Chart)
                    dto.MyBugsByStatus = await _dbBug.Bugs
                        .Where(b => testerBugIds.Contains(b.BugId))
                        .GroupBy(b => b.Status.StatusName)
                        .ToDictionaryAsync(g => g.Key, g => g.Count());

                    // Step 5: Severity of Bugs Tested (Donut Chart)
                    dto.SeverityOfBugsTested = await _dbBug.Bugs
                        .Where(b => testerBugIds.Contains(b.BugId))
                        .GroupBy(b => b.Severity)
                        .ToDictionaryAsync(g => g.Key, g => g.Count());

                    break;
            }

            return dto;
        }
    }
}
