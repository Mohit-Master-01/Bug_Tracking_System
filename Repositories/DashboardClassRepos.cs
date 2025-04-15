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
            var dto = new DashboardDTO
            {
                Role = role
            };

            switch (role)
            {
                case 4:
                    dto.TotalUsers = await _dbBug.Users.Where(u => u.RoleId != 4).CountAsync();
                    dto.TotalProjectManagers = await _dbBug.Users.CountAsync(u => u.Role.RoleId == 1);
                    dto.TotalDevelopers = await _dbBug.Users.CountAsync(u => u.Role.RoleId == 2);
                    dto.TotalTesters = await _dbBug.Users.CountAsync(u => u.Role.RoleId == 1);
                    dto.TotalProjects = await _dbBug.Projects.CountAsync();
                    dto.TotalBugs = await _dbBug.Bugs.CountAsync();

                    dto.UsersByRole = await _dbBug.Users
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

                    int? currentProjectId = _httpContextAccessor.HttpContext?.Session.GetInt32("CurrentProjectId");

                    // Get all project IDs assigned to the current PM
                    var myProjectIds = await _dbBug.UserProjects
                        .Where(t => t.UserId == userId)
                        .Select(t => t.ProjectId)
                        .Distinct()
                        .ToListAsync();

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
                    dto.TotalTesters = await _dbBug.Users.CountAsync(u => u.Role.RoleId == 1);

                    //dto.TotalTesters = await _dbBug.UserProjects
                    //    .Where(up => myProjectIds.Contains(up.ProjectId) && up.User.Role.RoleName == "Tester")
                    //    .Select(up => up.UserId)
                    //    .Distinct()
                    //    .CountAsync();
                    break;


                case 2:
                    var devBugIds = await _dbBug.TaskAssignments
                        .Where(t => t.AssignedTo == userId)
                        .Select(t => t.BugId)
                    .ToListAsync();

                    var devProjectIds = await _dbBug.TaskAssignments
                        .Where(t => t.AssignedTo == userId)
                        .Select(t => t.ProjectId)
                        .Distinct()
                        .ToListAsync();

                    dto.MyProjects = devProjectIds.Count;
                    dto.MyBugs = devBugIds.Count;

                    dto.MyBugsByStatus = await _dbBug.Bugs
                        .Where(b => devBugIds.Contains(b.BugId))
                        .GroupBy(b => b.Status.StatusName)
                        .ToDictionaryAsync(g => g.Key, g => g.Count());

                    dto.MyBugsBySeverity = await _dbBug.Bugs
                        .Where(b => devBugIds.Contains(b.BugId))
                        .GroupBy(b => b.Severity)
                        .ToDictionaryAsync(g => g.Key, g => g.Count());

                    dto.BugActivityTimeline = await _dbBug.Bugs
                        .Where(b => devBugIds.Contains(b.BugId))
                        .GroupBy(b => b.CreatedDate.Date)
                        .ToDictionaryAsync(g => g.Key.ToShortDateString(), g => g.Count());
                    break;

                case 3:
                    var testerBugIds = await _dbBug.TaskAssignments
                        .Where(t => t.AssignedTo == userId)
                        .Select(t => t.BugId)
                        .Distinct()
                        .ToListAsync();

                    dto.TotalBugsTested = testerBugIds.Count;

                    dto.VerifiedBugs = await _dbBug.Bugs
                        .CountAsync(b => testerBugIds.Contains(b.BugId) && b.Status.StatusName == "Verified");

                    dto.BugsToVerify = await _dbBug.Bugs
                        .CountAsync(b => testerBugIds.Contains(b.BugId) && b.Status.StatusName == "Resolved");

                    dto.BugVerificationHistory = await _dbBug.Bugs
                        .Where(b => testerBugIds.Contains(b.BugId))
                        .GroupBy(b => b.Status.StatusName)
                        .ToDictionaryAsync(g => g.Key, g => g.Count());

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
