using Bug_Tracking_System.Models;

namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface IAccountRepos
    {
        Task<object> AddUserRegister(User users, IFormFile? ImageFile);

        Task<bool> IsUsernameExist(string username);

        Task<bool> IsEmailExist(string email);

        Task<bool> OtpVerification(string Otp);

        Task<object> updateStatus(string Email);

        Task<User> GetUserDataByEmail(string email);

        string? GenerateDefaultProfileImage(string userName);

        Task<string> fetchEmail(string cred);

        Task<bool> IsVerified(string cred);
        //Task<List<Role>> GetRoles();
    }
}
