using Bug_Tracking_System.Models;
using X.PagedList;

namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface IBugRepos
    {
        Task<IPagedList<Bug>> GetAllBugsData(int pageNumber, int pageSize);
        
        Task<Bug> GetBugById(int bugId);

        Task<bool> UpdateBugStatus(int bugId, int statusId);

        Task<bool> SaveBug(Bug bug, List<IFormFile> attachments);

        Task<bool> DeleteBug(int bugId);
    }
}
