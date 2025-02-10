using Bug_Tracking_System.Models;

namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface IBugRepos
    {
        Task<List<Bug>> GetAllBugsData();
        
        Task<Bug> GetBugById(int bugId);

        Task<bool> UpdateBugStatus(int bugId, int statusId);

        Task<bool> SaveBug(Bug bug, List<IFormFile> attachments);

        Task<bool> DeleteBug(int bugId);
    }
}
