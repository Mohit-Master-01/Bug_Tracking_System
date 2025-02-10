using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bug_Tracking_System.Repositories
{
    public class ProfileClassRepos : IProfileRepos
    {
        private readonly DbBug _dbBug;
        private readonly IAccountRepos _acc;

        public ProfileClassRepos(DbBug dbBug, IAccountRepos acc)
        {
            _dbBug = dbBug;            
            _acc = acc;
        }

        public async Task<object> EditProfile(User user, IFormFile? ImageFile)
        {
            var UpdateProfile = await _dbBug.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);

            // Allowed file extensions
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

            // Handle image upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                // Get file extension
                string fileExtension = Path.GetExtension(ImageFile.FileName).ToLower();

                // Validate file extension and size (max 1MB)
                if (allowedExtensions.Contains(fileExtension) && ImageFile.Length <= 1 * 1024 * 1024)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProfileImage");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Ensure the uploads folder exists
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Save the image to the server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        ImageFile.CopyTo(stream);
                    }

                    // Update the image path
                    user.ProfileImage = "/ProfileImage/" + uniqueFileName;
                }
                else
                {
                    throw new Exception("Invalid file type or file size exceeds 1MB.");
                }
            }
            else
            {
                _acc.GenerateDefaultProfileImage(user.UserName);
            }
            if(user.UserId > 0)
            {
                UpdateProfile.UserName = user.UserName;
                UpdateProfile.Email = user.Email;
                if(ImageFile != null)
                {
                    UpdateProfile.ProfileImage = ImageFile.FileName;
                }                
            }
            ImageFile = null;
            user.ProfileImage = ImageFile.FileName;
            user.IsEmailVerified = true;
            string msg = "";
            if (user.UserId > 0)
            {
                _dbBug.Update(UpdateProfile);
                msg = "Your profile has been updated successfully";
            }
            else
            {
                msg = "There was an error updating your profile";
            }
            await _dbBug.SaveChangesAsync();

            return new { success = true, message = msg };

        }

        public async Task<User> GetAllUsersData()
        {
            //var user = await _dbBug.Users.FirstOrDefaultAsync(x => x.UserId == 3016);
            //return user;

            var user = await( from Users in _dbBug.Users
                       join Roles in _dbBug.Roles on Users.RoleId equals Roles.RoleId
                       where Users.UserId == 3016
                       select new User
                       {
                           UserId = Users.UserId,
                           UserName = Users.UserName,
                           Email = Users.Email,
                           PasswordHash = Users.PasswordHash,
                           RoleId = Roles.RoleId,
                           CreatedDate = DateTime.Now,
                           IsActive = true,
                           IsEmailVerified = true,
                           ProfileImage = Users.ProfileImage,
                           IsAdmin = true,
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
