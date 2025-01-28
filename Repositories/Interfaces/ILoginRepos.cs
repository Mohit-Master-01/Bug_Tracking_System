using System.Globalization;

namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface ILoginRepos
    {
        Task<object> AuthenticateUser(string EmailOrUsername, string Password);
        Task<object> TokenSenderViaEmail(string email);

        Task<object> ResetPassword(string creds, string newPassword);
    }
}
