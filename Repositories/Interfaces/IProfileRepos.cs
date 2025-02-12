using Bug_Tracking_System.Models;

namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface IProfileRepos
    {
        Task<User> GetAllUsersData(int userId);

        Task<object> EditProfile(User user, IFormFile? ImageFile);
        
    }
}
