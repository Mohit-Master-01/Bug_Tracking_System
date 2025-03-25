using Bug_Tracking_System.Models;
using X.PagedList;

namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface IMembersRepos
    {
        Task<User> GetAllMembersData();

        Task<IPagedList<User>> GetAllMembers(int pageNumber, int pageSize);

        Task<IPagedList<User>> GetAllProjectManagers(int pageNumber, int pageSize);

        Task<IPagedList<User>> GetAllDevelopers(int pageNumber, int pageSize);

        Task<IPagedList<User>> GetAllTesters(int pageNumber, int pageSize);

        Task<object> SaveMember(User member, IFormFile? ImageFile, List<int>? ProjectIds);

        Task<List<Role>> GetAllRoles();

        User checkExistence(string username, string email, int userId);

        string? GenerateDefaultProfileImage(string userName);

        string? GenerateRandomPassword();

    }
}
