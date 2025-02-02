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

        Task<int?> GetUserIdByEmail(string email);

        string? GenerateDefaultProfileImage(string userName);

        //Task<List<Role>> GetRoles();
    }
}
