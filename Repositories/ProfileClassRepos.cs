using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bug_Tracking_System.Repositories
{
    public class ProfileClassRepos : IProfileRepos
    {
        private readonly DbBug _dbBug;

        public ProfileClassRepos(DbBug dbBug)
        {
            _dbBug = dbBug;            
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
