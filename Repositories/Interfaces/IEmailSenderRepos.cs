namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface IEmailSenderRepos
    {
        Task SendEmailAsync(string toEmail, string subject, string body, string emailType);
        string GenerateOtp();
    }
}
