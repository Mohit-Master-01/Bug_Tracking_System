using Bug_Tracking_System.Models;
using X.PagedList;

namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface IProjectsRepos
    {
        Task<IPagedList<Project>> GetAllProjects(int pageNumber, int pageSize);

        Task<object> UpdateStatus(int projectId, bool status);

        Project checkExistance(string ProjectName, int ProjectId);

        Task<object> AddOrEditProject(Project projects);

        Task<bool> DeleteProject(int projectId);

        Task<IPagedList<Project>> GetUnassignedProjects(int pageNumber, int pageSize);  // ✅ Get all unassigned (new) bugs
        
        Task<List<User>> GetDevelopers();  // ✅ Get list of developers
        
        Task<bool> AssignProjectToDeveloper(int projectId, int developerId, int assignedBy);

        Task<Project> GetProjectById(int projectId);
    }
}
