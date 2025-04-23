using Bug_Tracking_System.Models;
using X.PagedList;

namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface IProjectsRepos
    {
        Task<List<Project>> GetAllProjects();

        Task<List<Project>> GetProjectByUser(int userId);

        Task<object> UpdateStatus(int projectId, bool status);

        Project checkExistance(string ProjectName, int ProjectId);

        Task<object> AddOrEditProject(Project projects);

        Task<bool> IsProjectExist(string projectname, int? projectId = null);

        Task<bool> DeleteProject(int projectId);

        Task<List<Project>> GetUnassignedProjects();  // ✅ Get all unassigned (new) bugs
        
        Task<List<User>> GetDevelopers();  // ✅ Get list of developers
        
        Task<bool> AssignProjectToDeveloper(int projectId, int developerId, int assignedBy);

        Task<Project> GetProjectById(int projectId);

    }
}
