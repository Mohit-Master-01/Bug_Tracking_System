using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bug_Tracking_System.Repositories.AuthClasses
{
    public class AccountClassRepos : IAccountRepos
    {
        private readonly DbBug _bug;
        private readonly IEmailSenderRepos _emailSender;

        public AccountClassRepos(DbBug bug, IEmailSenderRepos emailSender)
        {
            _bug = bug;
            _emailSender = emailSender;
        }

        public async Task<object> AddUserRegister(User users, IFormFile? ImageFile)
        {
            users.PasswordHash = BCrypt.Net.BCrypt.HashPassword(users.PasswordHash);

            users.CreatedDate = DateTime.Now;
            users.IsActive = true; // Activate the user by default

            users.Otp = _emailSender.GenerateOtp();
            users.OtpExpiry = DateTime.Now.AddMinutes(5);
            users.IsEmailVerified = false;
            string subj = "OTP Verification!!!";
            await _emailSender.SendEmailAsync(users.Email, subj, users.Otp,"Registration");

            await _bug.Users.AddAsync(users);
            await _bug.SaveChangesAsync();
            return new { success = true, message = "Check your email for the OTP verification" };
        }

        public async Task<int?> GetUserIdByEmail(string email)
        {
            return await _bug.Users
                .Where(u =>  u.Email == email)
                .Select(u => (int?)u.UserId)
                .FirstOrDefaultAsync(); //Retrieves the first match or null
        }

        //public async Task<List<Role>> GetRoles()
        //{
        //    return await _bug.Roles.ToListAsync();
        //}

        public async Task<bool> IsEmailExist(string email)
        {
            return await _bug.Users.AnyAsync(u => u.Email == email);
        }

        public Task<bool> IsUsernameExist(string username)
        {
            return _bug.Users.AnyAsync(u => u.UserName == username);
        }

        public async Task<bool> OtpVerification(string Otp)
        {
            return await _bug.Users.AnyAsync(u => u.Otp  == Otp && u.OtpExpiry > DateTime.Now);
        }

        public async Task<object> updateStatus(string Email)
        {
            var user = await _bug.Users.FirstOrDefaultAsync(u => u.Email == Email);
            if (user != null)
            {
                user.IsEmailVerified = true;
                await _bug.SaveChangesAsync();

                if ((bool)user.IsEmailVerified)
                {                          

                    // Send a success email to the user
                    string subject = "Email Verification Successful!";
                    
                    await _emailSender.SendEmailAsync(user.Email, subject, $"{user.UserName}", "VerificationSuccess");

                    return new { success = true, message = "Your email has been verified successfully, and a confirmation email has been sent." };
                }
                else
                {
                    return new { success = false, message = "Email is already verified." };
                }
            }

            return new { success = false, message = "Email not found" };
        }
    }
}
