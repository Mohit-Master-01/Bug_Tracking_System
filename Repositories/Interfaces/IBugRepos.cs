using Bug_Tracking_System.Models;
using X.PagedList;

namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface IBugRepos
    {
        Task<List<Bug>> GetBugsByProject(int projectId);

        Task<List<Bug>> GetAllBugs();

        Task<Bug> GetBugById(int bugId);

        Task<bool> UpdateBugStatus(int bugId, int statusId);

        Task<bool> SaveBug(Bug bug, List<IFormFile> attachments);

        Task<bool> DeleteBug(int bugId);

        Task<List<Bug>> GetUnassignedBugsByProject(int projectId); // ✅ Get all unassigned (new) bugs

        Task<List<Bug>> GetAllUnassignedBugs(); 

        Task<List<User>> GetDevelopers();  // ✅ Get list of developers

        Task<bool> AssignBugToDeveloper(int bugId, int developerId, int assignedBy);  // ✅ Assign bug to developer

        
    }
} 

