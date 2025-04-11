using Bug_Tracking_System.Models;

namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface ISidebarRepos
    {
        Task<List<SidebarModel>> GetTabsByRoleIdAsync(int roleId);

        List<Project> GetAssignedProjects(int userId);

    }
}
