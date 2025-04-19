using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;

namespace Bug_Tracking_System.Repositories.AuthClasses
{
    public class AccountClassRepos : IAccountRepos
    {
        private readonly DbBug _bug;
        private readonly IEmailSenderRepos _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountClassRepos(DbBug bug, IEmailSenderRepos emailSender, IHttpContextAccessor httpContextAccessor)
        {
            _bug = bug;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<object> AddUserRegister(User users, IFormFile? ImageFile)
        {
            users.PasswordHash = BCrypt.Net.BCrypt.HashPassword(users.PasswordHash);

            users.CreatedDate = DateTime.Now;
            users.IsActive = true; // Activate the user by default
            string msg = "Successfully Registered";
            string? gmail = _httpContextAccessor.HttpContext.Session.GetString("GoogleEmail");
            users.IsEmailVerified = true;
            if(gmail == null)
            {
                users.Otp = _emailSender.GenerateOtp();
                users.OtpExpiry = DateTime.Now.AddMinutes(5);

                users.IsEmailVerified = false;
                string subj = "OTP Verification!!!";
                await _emailSender.SendEmailAsync(users.Email, subj, users.Otp,"Registration");
                msg = "Check your email for the OTP verification";
            }
            if (ImageFile == null)
            {
                users.ProfileImage = GenerateDefaultProfileImage(users.UserName);
            }

            await _bug.Users.AddAsync(users);
            await _bug.SaveChangesAsync();
            return new { success = true, message =  msg};
        }

        public string GenerateDefaultProfileImage(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                userName = "U"; // Default to 'U' if username is empty

            string firstLetter = userName.Substring(0, 1).ToUpper();
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/ProfileImage");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string imagePath = Path.Combine(directoryPath, $"{userName}_profile.png");
            string relativePath = $"/ProfileImage/{userName}_profile.png"; // Path for DB storage

            int width = 200, height = 200;

            using (Bitmap bmp = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Gray); // Set background color

                    using (System.Drawing.Font font = new System.Drawing.Font("Arial", 80, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (SolidBrush textBrush = new SolidBrush(Color.White))
                    {
                        SizeF textSize = g.MeasureString(firstLetter, font);
                        PointF position = new PointF((width - textSize.Width) / 2, (height - textSize.Height) / 2);
                        g.DrawString(firstLetter, font, textBrush, position);
                    }
                }

                bmp.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
            }

            return relativePath;
        }

        public async Task<User> GetUserDataByEmail(string email)
        {
            return await _bug.Users
                .FirstOrDefaultAsync(x=> x.Email == email); //Retrieves the first match or null
        }

        public async Task<string> fetchEmail(string cred)
        {
            return await _bug.Users
                .Where(u => u.Email == cred || u.UserName == cred)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();
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

        public async Task<bool> IsVerified(string cred)
        {
            var user = await _bug.Users.FirstOrDefaultAsync(u => u.Email ==  cred || u.UserName == cred);
            return (bool)user.IsEmailVerified;
        }

        public async Task<string> fetchUsername(string cred)
        {
            return await _bug.Users
                .Where(u => u.Email == cred || u.UserName == cred)
                .Select(u => u.UserName)
                .FirstOrDefaultAsync();
        }

        public async Task<User> GetUserById(int userId)
        {
            return await _bug.Users
                .Where(u => u.UserId == userId)
                .FirstOrDefaultAsync();
        }

    }
}
