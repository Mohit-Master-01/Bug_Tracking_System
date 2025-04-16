using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Linq;
using X.PagedList;
using X.PagedList.Extensions;

namespace Bug_Tracking_System.Repositories.MembersClasses
{
    public class MemberClassRepos : IMembersRepos
    {
        private readonly DbBug _dbBug;
        private readonly IAccountRepos _acc;

        public MemberClassRepos(DbBug dbBug, IAccountRepos acc)
        {
            _dbBug = dbBug;
            _acc = acc;
        }

        public User checkExistence(string username, string email, int userId)
        {
            var existingMember = _dbBug.Users
            .Where(m => m.UserId != userId) //Exclude the current admin being edited
            .FirstOrDefault(m => m.Email == email || m.UserName == username);

            return existingMember;
        }

        public string? GenerateDefaultProfileImage(string userName)
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

        public string? GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@#!?";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<List<User>> GetAllDevelopers()
        {
            var members = await(
                                from Users in _dbBug.Users
                                join Roles in _dbBug.Roles on Users.RoleId equals Roles.RoleId
                                join Projects in _dbBug.Projects on Users.ProjectId equals Projects.ProjectId into projGroup
                                from project in projGroup.DefaultIfEmpty() // Left Join to include users without projects
                                where Users.RoleId == 2 && Users.IsActive == true // Exclude Admins
                                select new User
                                {
                                    UserId = Users.UserId,
                                    UserName = Users.UserName,
                                    Email = Users.Email,
                                    RoleId = Users.RoleId,
                                    CreatedDate = Users.CreatedDate,
                                    IsActive = Users.IsActive,
                                    ProfileImage = Users.ProfileImage,
                                    ProjectId = Users.ProjectId,
                                    Role = new Role
                                    {
                                        RoleId = Roles.RoleId,
                                        RoleName = Roles.RoleName
                                    },
                                    Project = project != null ? new Models.Project
                                    {
                                        ProjectId = project.ProjectId,
                                        ProjectName = project.ProjectName
                                    } : null
                                }
                            ).OrderByDescending(m => m.CreatedDate).ToListAsync();

            return members;
        }

        public async Task<List<User>> GetAllDevelopersByProject(int projectId)
        {
            var developers = await (
                        from up in _dbBug.UserProjects
                        join user in _dbBug.Users on up.UserId equals user.UserId
                        join role in _dbBug.Roles on user.RoleId equals role.RoleId
                        where up.ProjectId == projectId && user.RoleId == 2 && user.IsActive == true
                        select new User
                        {
                            UserId = user.UserId,
                            UserName = user.UserName,
                            Email = user.Email,
                            RoleId = user.RoleId,
                            CreatedDate = user.CreatedDate,
                            IsActive = user.IsActive,
                            ProfileImage = user.ProfileImage,
                            Role = new Role
                            {
                                RoleId = role.RoleId,
                                RoleName = role.RoleName
                            },
                            // You can set a single project or list of projects if needed
                            Project = new Models.Project
                            {
                                ProjectId = (int)up.ProjectId,
                                ProjectName = _dbBug.Projects
                                    .Where(p => p.ProjectId == up.ProjectId)
                                    .Select(p => p.ProjectName)
                                    .FirstOrDefault()
                            }
                        }
                    ).OrderByDescending(m => m.CreatedDate).ToListAsync();

            return developers;
        }

        public async Task<List<User>> GetAllMembers()
        {
            var members = await (
        from Users in _dbBug.Users
        join Roles in _dbBug.Roles on Users.RoleId equals Roles.RoleId
        join Projects in _dbBug.Projects on Users.ProjectId equals Projects.ProjectId into projGroup
        from project in projGroup.DefaultIfEmpty() // Left Join to include users without projects
        where Users.RoleId != 4 && Users.IsActive == true
        select new User
        {
            UserId = Users.UserId,
            UserName = Users.UserName,
            Email = Users.Email,
            RoleId = Users.RoleId,
            CreatedDate = Users.CreatedDate,
            IsActive = Users.IsActive,
            ProfileImage = Users.ProfileImage,
            ProjectId = Users.ProjectId,
            IsRestricted = Users.IsRestricted,
            Role = new Role
            {
                RoleId = Roles.RoleId,
                RoleName = Roles.RoleName
            },
            Project = project != null ? new Models.Project
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName
            } : null
        }
    ).OrderByDescending(m => m.CreatedDate).ToListAsync();

            return members;


            //var member = await _dbBug.Users.Include(m => m.Role).Where(m => m.RoleId == 2).Where(m => (bool)m.IsActive).Where(m => (bool)m.IsEmailVerified).OrderByDescending(m => m.CreatedDate).ToListAsync();
            //return member.ToPagedList(pageNumber, pageSize);

        }

        public async Task<List<User>> GetAllMembersByProject(int projectId)
        {
            var members = await (
                       from Users in _dbBug.Users
                       join Roles in _dbBug.Roles on Users.RoleId equals Roles.RoleId
                       join Projects in _dbBug.Projects on Users.ProjectId equals Projects.ProjectId
                       where Users.RoleId != 4 && Users.ProjectId == projectId && Users.IsActive == true // <-- filter by projectId
                       select new User
                       {
                           UserId = Users.UserId,
                           UserName = Users.UserName,
                           Email = Users.Email,
                           RoleId = Users.RoleId,
                           CreatedDate = Users.CreatedDate,
                           IsActive = Users.IsActive,
                           ProfileImage = Users.ProfileImage,
                           ProjectId = Users.ProjectId,
                           IsRestricted = Users.IsRestricted,
                           Role = new Role
                           {
                               RoleId = Roles.RoleId,
                               RoleName = Roles.RoleName
                           },
                           Project = new Models.Project
                           {
                               ProjectId = Projects.ProjectId,
                               ProjectName = Projects.ProjectName,
                           }

                       }).ToListAsync();

            return members;
        }


        public async Task<List<User>> GetAllProjectManagers()
        {
            var members = await (
                                from Users in _dbBug.Users
                                join Roles in _dbBug.Roles on Users.RoleId equals Roles.RoleId
                                join Projects in _dbBug.Projects on Users.ProjectId equals Projects.ProjectId into projGroup
                                from project in projGroup.DefaultIfEmpty() // Left Join to include users without projects
                                where Users.RoleId == 1 && Users.IsActive == true // Exclude Admins
                                select new User
                                {
                                    UserId = Users.UserId,
                                    UserName = Users.UserName,
                                    Email = Users.Email,
                                    RoleId = Users.RoleId,
                                    CreatedDate = Users.CreatedDate,
                                    IsActive = Users.IsActive,
                                    ProfileImage = Users.ProfileImage,
                                    ProjectId = Users.ProjectId,
                                    Role = new Role
                                    {
                                        RoleId = Roles.RoleId,
                                        RoleName = Roles.RoleName
                                    },
                                    Project = project != null ? new Models.Project
                                    {
                                        ProjectId = project.ProjectId,
                                        ProjectName = project.ProjectName
                                    } : null
                                }
                            ).OrderByDescending(m => m.CreatedDate).ToListAsync();

            return members;
        }

        public async Task<List<User>> GetAllProjectManagersByProject(int projectId)
        {
            var members = await(
                    from Users in _dbBug.Users
                    join Roles in _dbBug.Roles on Users.RoleId equals Roles.RoleId
                    join Projects in _dbBug.Projects on Users.ProjectId equals Projects.ProjectId
                    where Users.RoleId == 1 && Users.Project.ProjectId == projectId && Users.IsActive == true// Include Project managers
                    select new User
                    {
                        UserId = Users.UserId,
                        UserName = Users.UserName,
                        Email = Users.Email,
                        RoleId = Users.RoleId,
                        CreatedDate = Users.CreatedDate,
                        IsActive = Users.IsActive,
                        ProfileImage = Users.ProfileImage,
                        ProjectId = Users.ProjectId,
                        Role = new Role
                        {
                            RoleId = Roles.RoleId,
                            RoleName = Roles.RoleName
                        },                            
                        Project = new Models.Project
                        {
                            ProjectId = Projects.ProjectId,
                            ProjectName = Projects.ProjectName,
                        }
                    }
                ).OrderByDescending(m => m.CreatedDate).ToListAsync();

            return members;
        }

        public async Task<List<Role>> GetAllRoles()
        {
            return await _dbBug.Roles.ToListAsync();
        }

        public async Task<List<User>> GetAllTesters()
        {
            var members = await(
                                from Users in _dbBug.Users
                                join Roles in _dbBug.Roles on Users.RoleId equals Roles.RoleId
                                join Projects in _dbBug.Projects on Users.ProjectId equals Projects.ProjectId into projGroup
                                from project in projGroup.DefaultIfEmpty() // Left Join to include users without projects
                                where Users.RoleId == 3 && Users.IsActive == true // Exclude Admins
                                select new User
                                {
                                    UserId = Users.UserId,
                                    UserName = Users.UserName,
                                    Email = Users.Email,
                                    RoleId = Users.RoleId,
                                    CreatedDate = Users.CreatedDate,
                                    IsActive = Users.IsActive,
                                    ProfileImage = Users.ProfileImage,
                                    ProjectId = Users.ProjectId,
                                    Role = new Role
                                    {
                                        RoleId = Roles.RoleId,
                                        RoleName = Roles.RoleName
                                    },
                                    Project = project != null ? new Models.Project
                                    {
                                        ProjectId = project.ProjectId,
                                        ProjectName = project.ProjectName
                                    } : null
                                }
                            ).OrderByDescending(m => m.CreatedDate).ToListAsync();

            return members;
        }

        public async Task<List<User>> GetAllTestersByProject(int projectId)
        {
            var testers = await (
                        from up in _dbBug.UserProjects
                        join user in _dbBug.Users on up.UserId equals user.UserId
                        join role in _dbBug.Roles on user.RoleId equals role.RoleId
                        where up.ProjectId == projectId && user.RoleId == 2 && user.IsActive == true
                        select new User
                        {
                            UserId = user.UserId,
                            UserName = user.UserName,
                            Email = user.Email,
                            RoleId = user.RoleId,
                            CreatedDate = user.CreatedDate,
                            IsActive = user.IsActive,
                            ProfileImage = user.ProfileImage,
                            Role = new Role
                            {
                                RoleId = role.RoleId,
                                RoleName = role.RoleName
                            },
                            // You can set a single project or list of projects if needed
                            Project = new Models.Project
                            {
                                ProjectId = (int)up.ProjectId,
                                ProjectName = _dbBug.Projects
                                    .Where(p => p.ProjectId == up.ProjectId)
                                    .Select(p => p.ProjectName)
                                    .FirstOrDefault()
                            }
                        }
                    ).OrderByDescending(m => m.CreatedDate).ToListAsync();

            return testers;
        }

        public async Task<object> SaveMember(User member, IFormFile? ImageFile, List<int>? ProjectIds)
        {
            try
            {
                var existingMember = await _dbBug.Users
                    .Include(u => u.UserProjects)
                    .FirstOrDefaultAsync(u => u.UserId == member.UserId);

                bool isNewMember = existingMember == null;
                string tempPassword = "";

                if (isNewMember)
                {
                    member.ProfileImage = GenerateDefaultProfileImage(member.UserName);
                    tempPassword = GenerateRandomPassword();
                    member.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
                }

                member.IsAdmin = false;
                member.IsActive = true;

                if (isNewMember)
                {
                    await _dbBug.Users.AddAsync(member);
                    await _dbBug.SaveChangesAsync();  // Save here to get UserId for UserProjects mapping
                }
                else
                {
                    // Update user basic fields
                    existingMember.UserName = member.UserName;
                    existingMember.Email = member.Email;
                    existingMember.RoleId = member.RoleId;
                    existingMember.FirstName = member.FirstName;
                    existingMember.LastName = member.LastName;
                    existingMember.PhoneNumber = member.PhoneNumber;
                    existingMember.LinkedInProfile = member.LinkedInProfile;
                    existingMember.GitHubProfile = member.GitHubProfile;
                    existingMember.Skills = member.Skills;
                    existingMember.Bio = member.Bio;
                    await _dbBug.SaveChangesAsync();
                }

                // ✅ Handle UserProject mapping
                int userId = isNewMember ? member.UserId : existingMember.UserId;

                if (ProjectIds != null && ProjectIds.Any())
                {
                    var existingProjects = await _dbBug.UserProjects
                        .Where(up => up.UserId == userId)
                        .Select(up => up.ProjectId)
                        .ToListAsync();

                    var newProjectIds = ProjectIds.Except(existingProjects.Select(x => x.Value)).ToList();

                    if (!isNewMember && !newProjectIds.Any())
                    {
                        return new { success = false, message = "All selected projects are already assigned to this member." };
                    }

                    foreach (var projectId in newProjectIds)
                    {
                        var userProject = new UserProject
                        {
                            UserId = userId,
                            ProjectId = projectId
                        };
                        await _dbBug.UserProjects.AddAsync(userProject);
                    }
                    await _dbBug.SaveChangesAsync();
                }


                //if (ProjectIds != null && ProjectIds.Any())
                //{
                //    // Remove old mappings
                //    var oldMappings = _dbBug.UserProjects.Where(up => up.UserId == userId);
                //    _dbBug.UserProjects.RemoveRange(oldMappings);
                //    await _dbBug.SaveChangesAsync();

                //    // Add new mappings
                //    foreach (var projectId in ProjectIds)
                //    {
                //        var userProject = new UserProject
                //        {
                //            UserId = userId,
                //            ProjectId = projectId
                //        };
                //        await _dbBug.UserProjects.AddAsync(userProject);
                //    }
                //    await _dbBug.SaveChangesAsync();
                //}

                return new { success = true, message = isNewMember ? "New member added successfully" : "Member data updated successfully", tempPassword };
            }
            catch (Exception ex)
            {
                return new { success = false, message = "An error occurred: " + ex.Message };
            }
        }


        //public async Task<object> SaveMember(User member, IFormFile? ImageFile)
        //{
        //    try
        //    {
        //        var existingMember = await _dbBug.Users.FirstOrDefaultAsync(u => u.UserId == member.UserId);
        //        bool isNewMember = existingMember == null; // Check if adding or editing

        //        string tempPassword = "";

        //        // Assign default profile image only when adding a new member
        //        if (isNewMember)
        //        {
        //            member.ProfileImage = GenerateDefaultProfileImage(member.UserName);

        //            // Hash password only for new members
        //            tempPassword = GenerateRandomPassword();
        //            member.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword); // Hash the password
        //        }


        //        // Set default values
        //        member.IsAdmin = false;
        //        member.IsActive = true;

        //        if (isNewMember)
        //        {
        //            await _dbBug.Users.AddAsync(member);
        //        }
        //        else
        //        {
        //            // Update existing member fields (excluding ProfileImage)
        //            existingMember.UserName = member.UserName;
        //            existingMember.Email = member.Email;
        //            existingMember.ProjectId = member.ProjectId;
        //        }

        //        await _dbBug.SaveChangesAsync();

        //        return new { success = true, message = isNewMember ? "New member added successfully" : "Member data updated successfully", tempPassword };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new { success = false, message = "An error occurred: " + ex.Message };
        //    }

        //}

    }
}
