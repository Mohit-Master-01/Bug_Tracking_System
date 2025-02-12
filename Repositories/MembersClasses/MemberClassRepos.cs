using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
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

        public async Task<IPagedList<User>> GetAllDevelopers(int pageNumber, int pageSize)
        {
            var members = await(
                    from Users in _dbBug.Users
                    join Roles in _dbBug.Roles on Users.RoleId equals Roles.RoleId
                    where Users.RoleId == 2 // Exclude Admins
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
                        Projects = _dbBug.Projects
                                    .Where(p => p.Users.Any(au => au.UserId == Users.UserId))
                                    .Select(p => new Project
                                    {
                                        ProjectId = p.ProjectId,
                                        ProjectName = p.ProjectName
                                    }).ToList()
                    }
                ).OrderByDescending(m => m.CreatedDate).ToListAsync();

            return members.ToPagedList(pageNumber, pageSize);
        }

        public async Task<IPagedList<User>> GetAllMembers(int pageNumber, int pageSize)
        {
            var members = await (
        from Users in _dbBug.Users
        join Roles in _dbBug.Roles on Users.RoleId equals Roles.RoleId
        where Users.RoleId != 4 // Exclude Admins
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
            Projects = _dbBug.Projects
                        .Where(p => p.Users.Any(au => au.UserId == Users.UserId))
                        .Select(p => new Project
                        {
                            ProjectId = p.ProjectId,
                            ProjectName = p.ProjectName
                        }).ToList()
        }
    ).OrderByDescending(m => m.CreatedDate).ToListAsync();

            return members.ToPagedList(pageNumber, pageSize);


            //var member = await _dbBug.Users.Include(m => m.Role).Where(m => m.RoleId == 2).Where(m => (bool)m.IsActive).Where(m => (bool)m.IsEmailVerified).OrderByDescending(m => m.CreatedDate).ToListAsync();
            //return member.ToPagedList(pageNumber, pageSize);

        }

        public async Task<User> GetAllMembersData()
        {
            var members = await (
                          from Users in _dbBug.Users
                          join Roles in _dbBug.Roles on Users.RoleId equals Roles.RoleId
                          where Users.RoleId != 4
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
                              Projects = (ICollection<Project>)_dbBug.Projects
                                      .Where(p => p.Users.Any(au => au.UserId == Users.UserId)) // Fetch all projects assigned to the user
                                      .Select(p => new Project
                                      {
                                          ProjectId = p.ProjectId,
                                          ProjectName = p.ProjectName
                                      })
                          }).FirstOrDefaultAsync();

            return members;

        }

        public async Task<IPagedList<User>> GetAllProjectManagers(int pageNumber, int pageSize)
        {
            var members = await(
                    from Users in _dbBug.Users
                    join Roles in _dbBug.Roles on Users.RoleId equals Roles.RoleId
                    where Users.RoleId == 1 // Include Project managers
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
                        Projects = _dbBug.Projects
                                    .Where(p => p.Users.Any(au => au.UserId == Users.UserId))
                                    .Select(p => new Project
                                    {
                                        ProjectId = p.ProjectId,
                                        ProjectName = p.ProjectName
                                    }).ToList()
                    }
                ).OrderByDescending(m => m.CreatedDate).ToListAsync();

            return members.ToPagedList(pageNumber, pageSize);
        }

        public async Task<List<Role>> GetAllRoles()
        {
            return await _dbBug.Roles.ToListAsync();
        }

        public async Task<IPagedList<User>> GetAllTesters(int pageNumber, int pageSize)
        {
            var members = await(
                    from Users in _dbBug.Users
                    join Roles in _dbBug.Roles on Users.RoleId equals Roles.RoleId
                    where Users.RoleId == 3 // Exclude Admins
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
                        Projects = _dbBug.Projects
                                    .Where(p => p.Users.Any(au => au.UserId == Users.UserId))
                                    .Select(p => new Project
                                    {
                                        ProjectId = p.ProjectId,
                                        ProjectName = p.ProjectName
                                    }).ToList()
                    }
                ).OrderByDescending(m => m.CreatedDate).ToListAsync();

            return members.ToPagedList(pageNumber, pageSize);
        }

        public async Task<object> SaveMember(User member, IFormFile? ImageFile)
        {
            try
            {
                var existingMember = await _dbBug.Users.FirstOrDefaultAsync(u => u.UserId == member.UserId);
                bool isNewMember = existingMember == null; // Check if adding or editing

                string tempPassword = "";

                // Assign default profile image only when adding a new member
                if (isNewMember)
                {
                    member.ProfileImage = GenerateDefaultProfileImage(member.UserName);

                    // Hash password only for new members
                    tempPassword = GenerateRandomPassword();
                    member.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword); // Hash the password
                }
                               
               
                // Set default values
                member.IsAdmin = false;
                member.IsActive = true;

                if (isNewMember)
                {
                    await _dbBug.Users.AddAsync(member);
                }
                else
                {
                    // Update existing member fields (excluding ProfileImage)
                    existingMember.UserName = member.UserName;
                    existingMember.Email = member.Email;
                    existingMember.ProjectId = member.ProjectId;
                }

                await _dbBug.SaveChangesAsync();

                return new { success = true, message = isNewMember ? "New member added successfully" : "Member data updated successfully", tempPassword };
            }
            catch (Exception ex)
            {
                return new { success = false, message = "An error occurred: " + ex.Message };
            }

        }

    }
}
