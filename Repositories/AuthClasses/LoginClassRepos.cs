using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Bug_Tracking_System.Repositories.AuthClasses
{
    public class LoginClassRepos : ILoginRepos
    {

        private readonly DbBug _dbBug;
        private readonly IEmailSenderRepos _emailSender;
        private readonly IMemoryCache _memoryCache;

        public LoginClassRepos(DbBug dbBug, IEmailSenderRepos emailSender, IMemoryCache memoryCache)
        {
            _dbBug = dbBug;
            _emailSender = emailSender;
            _memoryCache = memoryCache;
        }

        public async Task<object> AuthenticateUser(string EmailOrUsername, string Password)
        {

            const int maxAttempts = 5; //Maximum allowed attempts
            const int lockoutDurationSeconds = 300; //Lockout duration in seconds

            //Define cache keys for tracking attempts and lockout status
            var attemptKey = $"LoginAttempts_{EmailOrUsername}";
            var lockoutKey = $"Lockout_{EmailOrUsername}";

            //Fetching user by email or username
            var user = await _dbBug.Users.FirstOrDefaultAsync(u => u.Email == EmailOrUsername || u.UserName == EmailOrUsername);
            
            if (user == null)
            {
                return new { success = false, message = "User not found" };
            }

            //Comparing hashed passwords
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash);
            if (isPasswordValid)
            {
                _memoryCache.Remove(attemptKey);
                return new { success = true, message = "You are successfully logged in", email = user.Email };
            }

            //Check if the user is locked out
            if(_memoryCache.TryGetValue(lockoutKey, out DateTime lockoutEndTime) && lockoutEndTime > DateTime.Now)
            {
                var remainingTime = (int)(lockoutEndTime - DateTime.Now).TotalSeconds;
                return new { success = false, message = $"Account is locked. Try again in {remainingTime} seconds." };
            }

            int attempts = _memoryCache.GetOrCreate(attemptKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(lockoutDurationSeconds);
                return 0;
            });

            attempts++;
            _memoryCache.Set(attemptKey, attempts, TimeSpan.FromSeconds(lockoutDurationSeconds));

            //Lockout the user if max attempts reached
            if(attempts >= maxAttempts)
            {
                _memoryCache.Set(lockoutKey, DateTime.Now.AddSeconds(lockoutDurationSeconds), TimeSpan.FromSeconds(lockoutDurationSeconds));
                _memoryCache.Remove(attemptKey); //Reset attempts after lockout
                return new { success = false, message = $"Account is locked. Try again in {lockoutDurationSeconds} seconds." };

            }

            return new { success = false, message = $"Invalid credentials. You have {maxAttempts - attempts} attempts left." };
        }

        public async Task<object> ResetPassword(string creds, string newPassword)
        {
            var user = _dbBug.Users.FirstOrDefault(u => u.Email == creds || u.UserName == creds);
            if(user != null)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                _dbBug.SaveChanges();
                return new { success = true, message = "Password Changed Successfully" };
            }

            return new { success = false, message = "User not found" };
        }

        public async Task<object> TokenSenderViaEmail(string email)
        {
            var user = await _dbBug.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return new { success = false, message = "No user found with that email or username" };
            }

            //We are creating token variables here
            var resetToken = Guid.NewGuid().ToString();
            var expirationTime = DateTime.Now.AddDays(1); //Token expires in an hour

            //Store the token and expiration time in-memory
            _memoryCache.Set(resetToken, new { UserId = user.UserId, ExpirationTime = expirationTime }, expirationTime);

            //Create the password reset URL with the token
            var resetUrl = "http://localhost:5088/Account/ResetPassword?token=" + resetToken;

            //Send the reset email
            await _emailSender.SendEmailAsync(user.Email, "Password Reset Request",
                $"Click <a href='{resetUrl}'>here</a> to reset your password.", "ForgotPassword");

            return new { success = true, message = "Password reset link sent to your email." };
        }


    }

}
