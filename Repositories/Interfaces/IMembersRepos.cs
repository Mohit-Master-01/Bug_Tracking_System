using Bug_Tracking_System.Models;
using X.PagedList;

namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface IMembersRepos
    {
        Task<User> GetAllMembersData();

        Task<List<User>> GetAllMembers();

        Task<List<User>> GetAllProjectManagersByProject(int projectId);

        Task<List<User>> GetAllProjectManagers();

        Task<List<User>> GetAllDevelopersByProject(int projectId);
            
        Task<List<User>> GetAllDevelopers();

        Task<List<User>> GetAllTestersByProject(int projectId);
        Task<List<User>> GetAllTesters();

        Task<object> SaveMember(User member, IFormFile? ImageFile, List<int>? ProjectIds);

        Task<List<Role>> GetAllRoles();

        User checkExistence(string username, string email, int userId);

        string? GenerateDefaultProfileImage(string userName);

        string? GenerateRandomPassword();

    }
}
