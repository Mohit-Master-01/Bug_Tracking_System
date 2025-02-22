using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bug_Tracking_System.Repositories
{
    public class ProfileClassRepos : IProfileRepos
    {
        private readonly DbBug _dbBug;
        private readonly IAccountRepos _acc;
        private readonly IEmailSenderRepos _emailSender;

        public ProfileClassRepos(DbBug dbBug, IAccountRepos acc, IEmailSenderRepos emailSender)
        {
            _dbBug = dbBug;            
            _acc = acc;
            _emailSender = emailSender;
        }

        public async Task<object> EditProfile(User user, IFormFile? ImageFile)
        {
            var UpdateProfile = await _dbBug.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);

            if (UpdateProfile == null)
            {
                return new { success = false, message = "User not found" };
            }

            // Allowed file extensions
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

            if (ImageFile != null && ImageFile.Length > 0)
            {
                string fileExtension = Path.GetExtension(ImageFile.FileName).ToLower();

                if (allowedExtensions.Contains(fileExtension) && ImageFile.Length <= 1 * 1024 * 1024) // 1MB limit
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProfileImage");

                    // **Delete old image if exists**
                    if (!string.IsNullOrEmpty(UpdateProfile.ProfileImage))
                    {
                        string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", UpdateProfile.ProfileImage.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // **Save new image**
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    UpdateProfile.ProfileImage = "/ProfileImage/" + uniqueFileName;
                }
                else
                {
                    return new { success = false, message = "Invalid file type or file size exceeds 1MB." };
                }
            }

            // Update other profile details
            UpdateProfile.UserName = user.UserName;
            UpdateProfile.Email = user.Email;
            UpdateProfile.FirstName = user.FirstName;
            UpdateProfile.LastName = user.LastName;
            UpdateProfile.PhoneNumber = user.PhoneNumber;
            UpdateProfile.LinkedInProfile = user.LinkedInProfile;
            UpdateProfile.GitHubProfile = user.GitHubProfile;
            UpdateProfile.Bio = user.Bio;
            UpdateProfile.Skills = user.Skills;

            await _dbBug.SaveChangesAsync();

            return new { success = true, message = "Your profile has been updated successfully" };
        }


        //public async Task<object> EditProfile(User user, IFormFile? ImageFile)
        //{
        //    var UpdateProfile = await _dbBug.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);

        //    if (UpdateProfile == null)
        //    {
        //        return new { success = false, message = "User not found" };
        //    }

        //    // Allowed file extensions
        //    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

        //    // Handle image upload
        //    if (ImageFile != null && ImageFile.Length > 0)
        //    {
        //        string fileExtension = Path.GetExtension(ImageFile.FileName).ToLower();

        //        if (allowedExtensions.Contains(fileExtension) && ImageFile.Length <= 1 * 1024 * 1024)
        //        {
        //            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProfileImage");
        //            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
        //            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //            if (!Directory.Exists(uploadsFolder))
        //            {
        //                Directory.CreateDirectory(uploadsFolder);
        //            }

        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await ImageFile.CopyToAsync(stream);
        //            }

        //            UpdateProfile.ProfileImage = "/ProfileImage/" + uniqueFileName;
        //        }
        //        else
        //        {
        //            return new { success = false, message = "Invalid file type or file size exceeds 1MB." };
        //        }
        //    }

        //    // Update other fields
        //    UpdateProfile.UserName = user.UserName;
        //    UpdateProfile.Email = user.Email;
        //    UpdateProfile.FirstName = user.FirstName;
        //    UpdateProfile.LastName = user.LastName;
        //    UpdateProfile.PhoneNumber = user.PhoneNumber;
        //    UpdateProfile.LinkedInProfile = user.LinkedInProfile;
        //    UpdateProfile.GitHubProfile = user.GitHubProfile;
        //    UpdateProfile.Bio = user.Bio;
        //    UpdateProfile.Skills = user.Skills;

        //    await _dbBug.SaveChangesAsync();

        //    return new { success = true, message = "Your profile has been updated successfully" };

        //}

        public async Task<User> GetAllUsersData(int userId)
        {
            //var user = await _dbBug.Users.FirstOrDefaultAsync(x => x.UserId == 3016);
            //return user;

            var user = await( from Users in _dbBug.Users
                       join Roles in _dbBug.Roles on Users.RoleId equals Roles.RoleId
                       where Users.UserId == userId 
                       select new User
                       {
                           UserId = Users.UserId,
                           UserName = Users.UserName,
                           FirstName = Users.FirstName,
                           LastName = Users.LastName,
                           PhoneNumber = Users.PhoneNumber,
                           LinkedInProfile = Users.LinkedInProfile,
                           GitHubProfile = Users.GitHubProfile,
                           Bio = Users.Bio,
                           Skills = Users.Skills,
                           Email = Users.Email,
                           PasswordHash = Users.PasswordHash,
                           RoleId = Roles.RoleId,
                           CreatedDate = Users.CreatedDate,
                           IsActive = Users.IsActive,
                           IsEmailVerified = Users.IsEmailVerified,
                           ProfileImage = Users.ProfileImage,
                           IsAdmin = Users.IsAdmin,
                           ProjectId = Users.ProjectId,
                           Role = new Role
                           {
                               RoleId = Roles.RoleId,
                               RoleName = Roles.RoleName
                           },
                           Projects = _dbBug.Projects
                                      .Where(p => p.Users.Any(au => au.UserId == Users.UserId)) // Fetch all projects assigned to the user
                                      .Select(p => new Project
                                      {
                                          ProjectId = p.ProjectId,
                                          ProjectName = p.ProjectName
                                      }).ToList()
                       }).FirstOrDefaultAsync();

            return user;

        }
               

        public async Task<object> UpdateEmailVerification(User users)
        {
            // Fetch the existing user from the database based on email
            var existingUser = await _dbBug.Users.FirstOrDefaultAsync(u => u.Email == users.Email);

            // Check if the user exists
            if (existingUser == null)
            {
                return new { success = false, message = "User not found" };
            }

            // Update OTP-related fields
            existingUser.Otp = _emailSender.GenerateOtp();
            existingUser.OtpExpiry = DateTime.Now.AddMinutes(5);
            existingUser.IsEmailVerified = false;

            string subj = "OTP Verification!!!";
            await _emailSender.SendEmailAsync(existingUser.Email, subj, existingUser.Otp, "Registration");

            // Save changes
            await _dbBug.SaveChangesAsync();

            return new { success = true, message = "Check your email for the OTP verification" };

        }

        public async Task<bool> OtpVerification(string Otp)
        {
            return await _dbBug.Users.AnyAsync(u => u.Otp == Otp && u.OtpExpiry > DateTime.Now);
        }

        public async Task<object> updateStatus(string Email)
        {
            var user = await _dbBug.Users.FirstOrDefaultAsync(u => u.Email == Email);
            if (user != null)
            {
                user.IsEmailVerified = true;
                await _dbBug.SaveChangesAsync();

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

        //public async Task<User> GetAllUsersData()
        //{
        //    var user = await _dbBug.Users
        //        .Include(u => u.Role)  // Ensure Role is loaded
        //        .Include(u => u.Project)  // Ensure Project is loaded
        //        .FirstOrDefaultAsync(x => x.UserId == 3016);

        //    return user;
        //}

    }
}
