using Bug_Tracking_System.Models;

namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface IProfileRepos
    {
        Task<User> GetAllUsersData(int userId);

        Task<User?> EditProfile(User user, IFormFile? ImageFile);

        //Task<object> EditProfile(User user, IFormFile? ImageFile);

        Task<object> UpdateEmailVerification(User users);

        Task<bool> OtpVerification(string Otp);

        Task<object> updateStatus(string Email);

    }
}
