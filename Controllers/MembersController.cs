using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Composition;
using System.Data;
using System.Net.Mail;

namespace Bug_Tracking_System.Controllers
{
    public class MembersController : BaseController
    {
        private readonly DbBug _dbBug;
        private readonly IMembersRepos _member;
        private readonly IAccountRepos _acc;
        private readonly IEmailSenderRepos _emailSender;
        private readonly IPermissionHelperRepos _permission;
        private readonly IExportRepos _export;
        private readonly IImportRepos _import;

        public MembersController(DbBug dbBug, IExportRepos export, IImportRepos import, IPermissionHelperRepos permission, IMembersRepos member, IEmailSenderRepos emailSender, IAccountRepos acc, ISidebarRepos sidebar) : base(sidebar)
        {
            _dbBug = dbBug;
            _member = member;
            _acc = acc;
            _emailSender = emailSender;
            _permission = permission;
            _export = export;
            _import = import;
        }

        // Public method to get user permission
        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess(action, roleId);
            ViewBag.PermissionType = permissionType;
            return permissionType;
        }


        [ActionName("MembersList")]
        public async Task<IActionResult> Index(int? page)
        {
            string permissionType = GetUserPermission("Manage Members");
            if (permissionType
                == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                int pageSize = 4; // Number of records per page
                int pageNumber = page ?? 1; // Default to page 1

                ViewBag.PageTitle = "Members List";
                ViewBag.Breadcrumb = "Reports";
                var members = await _member.GetAllMembers(pageNumber, pageSize);
                return View(members);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }



        public async Task<IActionResult> ExportMemberList()
        {
            var members = await (
                    from Users in _dbBug.Users
                    join Roles in _dbBug.Roles on Users.RoleId equals Roles.RoleId
                    join Projects in _dbBug.Projects on Users.ProjectId equals Projects.ProjectId into projGroup
                    from project in projGroup.DefaultIfEmpty() // Left Join to include users without projects
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
                        Project = project != null ? new Project
                        {
                            ProjectId = project.ProjectId,
                            ProjectName = project.ProjectName
                        } : null
                    }
                ).OrderByDescending(m => m.CreatedDate).ToListAsync();


            var dataTable = new DataTable("MembersListReports");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Member Id"),
                new DataColumn("Username"),
                new DataColumn("Email"),
                new DataColumn("Role"),
                new DataColumn("Joining Date"),
                new DataColumn("Status")
            });

            foreach (var member in members)
            {
                dataTable.Rows.Add(member.UserId, member.UserName, member.Email, member.Role?.RoleName, member.CreatedDate, member.IsActive);
            }

            var fileBytes = _export.ExportToExcel(dataTable, "MembersListReports");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MembersListReports.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ProjectManagersList(int? page)
        {
            string permissionType = GetUserPermission("View Project Managers");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                int pageSize = 4;
                int pageNumber = page ?? 1;
                ViewBag.PageTitle = "Project Managers List";
                ViewBag.Breadcrumb = "Reports";

                var ProjectManagers = await _member.GetAllProjectManagers(pageNumber, pageSize);

                ViewBag.TotalManagers = ProjectManagers.Count;
                ViewBag.ActiveManagers = ProjectManagers.Count(u => (bool)u.IsActive);
                ViewBag.InactiveManagers = ProjectManagers.Count(u => (bool)!u.IsActive);

                ViewBag.JoinedThisMonth = ProjectManagers.Count(u =>
                                            u.CreatedDate.HasValue &&
                                            u.CreatedDate.Value.Month == DateTime.Now.Month &&
                                            u.CreatedDate.Value.Year == DateTime.Now.Year);


                return View(ProjectManagers);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        public async Task<IActionResult> ExportProjectManagerList()
        {
            var projectManagers = await (
                    from Users in _dbBug.Users
                    join Roles in _dbBug.Roles on Users.RoleId equals Roles.RoleId
                    join Projects in _dbBug.Projects on Users.ProjectId equals Projects.ProjectId
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
                        Project = new Project
                        {
                            ProjectId = Projects.ProjectId,
                            ProjectName = Projects.ProjectName,
                        }
                    }
                ).OrderByDescending(m => m.CreatedDate).ToListAsync();


            var dataTable = new DataTable("ProjectManagerListReports");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Member Id"),
                new DataColumn("Username"),
                new DataColumn("Email"),
                new DataColumn("Role"),
                new DataColumn("Projects"),
                new DataColumn("Joining Date"),
                new DataColumn("Status")
            });

            foreach (var projectManager in projectManagers)
            {
                dataTable.Rows.Add(projectManager.UserId, projectManager.UserName, projectManager.Email, projectManager.Role?.RoleName, projectManager.Project?.ProjectName ?? "Not Assigned", projectManager.CreatedDate, projectManager.IsActive);
            }

            var fileBytes = _export.ExportToExcel(dataTable, "ProjectManagerListReports");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProjectManagerListReports.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> DevelopersList(int? page)
        {
            string permissionType = GetUserPermission("View Developers");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {
                int pageSize = 4;
                int pageNumber = page ?? 1;
                ViewBag.PageTitle = "Developers List";
                ViewBag.Breadcrumb = "Reports";

                var developers = await _member.GetAllDevelopers(pageNumber, pageSize);

                ViewBag.TotalDevelopers = developers.Count;
                ViewBag.ActiveDevelopers = developers.Count(u => (bool)u.IsActive);
                ViewBag.InactiveDevelopers = developers.Count(u => (bool)!u.IsActive);

                ViewBag.JoinedThisMonth = developers.Count(u =>
                                            u.CreatedDate.HasValue &&
                                            u.CreatedDate.Value.Month == DateTime.Now.Month &&
                                            u.CreatedDate.Value.Year == DateTime.Now.Year);

                if (developers == null || !developers.Any())
                {
                    ViewBag.Message = "No developers found.";
                }

                return View(developers);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        public async Task<IActionResult> ExportDevelopersList()
        {
            var developers = await (
                    from Users in _dbBug.Users
                    join Roles in _dbBug.Roles on Users.RoleId equals Roles.RoleId
                    join Projects in _dbBug.Projects on Users.ProjectId equals Projects.ProjectId into projGroup
                    from project in projGroup.DefaultIfEmpty() // Left Join to include users without projects
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
                        Project = project != null ? new Project
                        {
                            ProjectId = project.ProjectId,
                            ProjectName = project.ProjectName
                        } : null
                    }
                ).OrderByDescending(m => m.CreatedDate).ToListAsync();


            var dataTable = new DataTable("DevelopersListReports");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Member Id"),
                new DataColumn("Username"),
                new DataColumn("Email"),
                new DataColumn("Role"),
                new DataColumn("Projects"),
                new DataColumn("Joining Date"),
                new DataColumn("Status")
            });

            foreach (var developer in developers)
            {
                dataTable.Rows.Add(developer.UserId, developer.UserName, developer.Email, developer.Role?.RoleName, developer.Project?.ProjectName ?? "Not Assigned", developer.CreatedDate, developer.IsActive);
            }

            var fileBytes = _export.ExportToExcel(dataTable, "DevelopersListReports");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DevelopersListReports.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> TestersList(int? page)
        {
            string permissionType = GetUserPermission("View Testers");
            if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {

                int pageSize = 4;
                int pageNumber = page ?? 1;
                ViewBag.PageTitle = "Tester List";
                ViewBag.Breadcrumb = "Reports";
                var Testers = await _member.GetAllTesters(pageNumber, pageSize);

                ViewBag.TotalTesters = Testers.Count;
                ViewBag.ActiveTesters = Testers.Count(u => (bool)u.IsActive);
                ViewBag.InactiveTesters = Testers.Count(u => (bool)!u.IsActive);

                ViewBag.JoinedThisMonth = Testers.Count(u =>
                                            u.CreatedDate.HasValue &&
                                            u.CreatedDate.Value.Month == DateTime.Now.Month &&
                                            u.CreatedDate.Value.Year == DateTime.Now.Year);

                if (Testers == null || !Testers.Any())
                {
                    ViewBag.Message = "No developers found.";
                }
                return View(Testers);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        public async Task<IActionResult> ExportTestersList()
        {
            var testers = await (
                    from Users in _dbBug.Users
                    join Roles in _dbBug.Roles on Users.RoleId equals Roles.RoleId
                    join Projects in _dbBug.Projects on Users.ProjectId equals Projects.ProjectId into projGroup
                    from project in projGroup.DefaultIfEmpty() // Left Join to include users without projects
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
                        Project = project != null ? new Project
                        {
                            ProjectId = project.ProjectId,
                            ProjectName = project.ProjectName
                        } : null
                    }
                ).OrderByDescending(m => m.CreatedDate).ToListAsync();


            var dataTable = new DataTable("TestersListReports");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Member Id"),
                new DataColumn("Username"),
                new DataColumn("Email"),
                new DataColumn("Role"),
                new DataColumn("Projects"),
                new DataColumn("Joining Date"),
                new DataColumn("Status")
            });

            foreach (var tester in testers)
            {
                dataTable.Rows.Add(tester.UserId, tester.UserName, tester.Email, tester.Role?.RoleName, tester.Project?.ProjectName ?? "Not Assigned", tester.CreatedDate, tester.IsActive);
            }

            var fileBytes = _export.ExportToExcel(dataTable, "TestersListReports");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TestersListReports.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> MemberDetails(int id)
        {
            ViewBag.PageTitle = "Member's Details";
            ViewBag.Breadcrumb = "Manage Member";

            var member = await _dbBug.Users.Include(m => m.Role).FirstOrDefaultAsync(m => m.UserId == id);
            if (member == null)
                return NotFound();

            // Assuming a many-to-many relationship (UserProjects table)
            var projects = await _dbBug.Projects
                .Where(p => p.Users.Any(pd => pd.UserId == id))
                .ToListAsync();

            ViewBag.AssignedProjects = projects;

            return View(member);
        }



        // API Endpoint to Fetch Projects Dynamically
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var projects = await _dbBug.Projects
                                       .Select(p => new { p.ProjectId, p.ProjectName })
                                       .ToListAsync();

            return Json(projects);
        }

        //Generate random password
        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@#!?";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HttpGet]
        public async Task<IActionResult> SaveMember(int? id)
        {

            string permissionType = GetUserPermission("Manage Members");
            if (permissionType == "canEdit" || permissionType == "fullAccess")
            {
                ViewBag.Breadcrumb = "Manage Members";

                if (id == null)
                {
                    ViewBag.PageTitle = "Add a Member";
                }
                else
                {
                    ViewBag.PageTitle = "Edit Member";

                }

                User user = new User();
                if (id > 0)
                {
                    user = await _dbBug.Users.FirstOrDefaultAsync(u => u.UserId == id);
                }

                // Fetch roles dynamically excluding Admin (RoleId = 4)
                ViewBag.RoleId = new SelectList(await _dbBug.Roles
                                                .Where(r => r.RoleId != 4)
                                                .Select(r => new { r.RoleId, r.RoleName })
                                                .ToListAsync(), "RoleId", "RoleName");

                // Fetch projects dynamically
                ViewBag.Projects = new SelectList(await _dbBug.Projects
                                                .Select(p => new { p.ProjectId, p.ProjectName })
                                                .ToListAsync(), "ProjectId", "ProjectName");

                return View(user);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        public async Task<IActionResult> SaveMember(User member, IFormFile? ImageFile, List<int>? ProjectIds)
        {
            try
            {
                if (member.UserId > 0)
                {
                    User resData = _member.checkExistence(member.UserName, member.Email, member.UserId);
                    if (resData != null)
                    {
                        if (resData.Email == member.Email)
                            return Json(new { success = false, message = "Email already exists" });
                        if (resData.UserName == member.UserName)
                            return Json(new { success = false, message = "Username already exists" });
                    }
                }
                else
                {
                    if (await _acc.IsUsernameExist(member.UserName))
                        return Json(new { success = false, message = "Username already exists" });
                    if (await _acc.IsEmailExist(member.Email))
                        return Json(new { success = false, message = "Email already exists" });
                }

                var res = await _member.SaveMember(member, ImageFile, ProjectIds);

                if (((dynamic)res).success)
                {
                    // ✅ Fetch Role Name
                    string roleName = await _dbBug.Roles
                        .Where(r => r.RoleId == member.RoleId)
                        .Select(r => r.RoleName)
                        .FirstOrDefaultAsync() ?? "Not Assigned";

                    var projectNames = (ProjectIds != null && ProjectIds.Any())
                        ? await _dbBug.Projects
                            .Where(p => ProjectIds.Contains(p.ProjectId))
                            .Select(p => p.ProjectName)
                            .ToListAsync()
                        : new List<string>();

                    string assignedProjects = projectNames.Any() ? string.Join(", ", projectNames) : "Not Assigned";


                    // ✅ Get Temporary Password
                    string tempPassword = ((dynamic)res).tempPassword ?? "Not Available";

                    string subject = "Welcome to Bugify - Your Account is Ready!";
                    string body = $@"
                <div class='container'>                                    
                    <div class='content'>
                        <p>Dear {member.UserName},</p>
                        <p>We are excited to welcome you to <b>Bugify</b> - our efficient Bug Tracking System.</p>
                        <p>Your account has been successfully created with the following details:</p>
                        <div class='info'>
                            <p><b>Role:</b> {roleName}</p>
                            <p><b>Assigned Projects:</b> {assignedProjects}</p>
                            <p><b>Username:</b> {member.UserName}</p>
                            <p><b>Email:</b> {member.Email}</p>
                            <p><b>Temporary Password:</b> {tempPassword}</p>
                        </div>
                        <p>You can log in using the above credentials and change your password after logging in.</p>
                        <p>If you have any questions, feel free to contact our support team.</p>
                        <p>We look forward to your contributions!</p>
                    </div>
                    <div class='footer'>
                        &copy; {DateTime.Now.Year} Bugify. All rights reserved.
                    </div>
                </div>";

                    await _emailSender.SendEmailAsync(member.Email, subject, body, "NewMemberAdded");
                }

                return Ok(res);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Json(new { success = false, message = "Unknown error occurred" });
            }
        }


        //public async Task<IActionResult> SaveMember(User member, IFormFile? ImageFile)
        //{
        //    try
        //    {
        //        if(member.UserId > 0)
        //        {
        //            User resData = _member.checkExistence(member.UserName, member.Email, member.UserId);
        //            if(resData != null)
        //            {
        //                if (resData.Email == member.Email)
        //                {
        //                    return Json(new { success = false, message = "Email already exists" });
        //                }
        //                if (resData.UserName == member.UserName)
        //                {
        //                    return Json(new { success = false, message = "Username already exists" });
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (await _acc.IsUsernameExist(member.UserName))
        //            {
        //                return Json(new { success = false, message = "Username already exists - edit" });
        //            }
        //            if (await _acc.IsEmailExist(member.Email))
        //            {
        //                return Json(new { success = false, message = "Email already exists - edit" });
        //            }
        //        }
        //        string username = member.UserName;
        //        //// **Generate a Temporary Password for New User**
        //        //string tempPassword = GenerateRandomPassword();
        //        ////member.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
        //        //string password = tempPassword;
        //        string email = member.Email;
        //        int id = member.UserId;

        //        int ResId = (int)HttpContext.Session.GetInt32("UserId");

        //        var res = await _member.SaveMember(member, ImageFile);

        //        if (((dynamic)res).success)
        //        {
        //            // ✅ **Fetch Role Name**
        //            string roleName = await _dbBug.Roles
        //                .Where(r => r.RoleId == member.RoleId)
        //                .Select(r => r.RoleName)
        //                .FirstOrDefaultAsync() ?? "Not Assigned";

        //            // ✅ **Fetch Project Name**
        //            string projectName = await _dbBug.Projects
        //                .Where(p => p.ProjectId == member.ProjectId)
        //                .Select(p => p.ProjectName)
        //                .FirstOrDefaultAsync() ?? "Not Assigned";

        //            // ✅ **Get Temporary Password**
        //            var responseObj = (dynamic)res;
        //            string tempPassword = responseObj.tempPassword ?? "Not Available";

        //            string subject = "Welcome to Bugify - Your Account is Ready!";
        //            string body = $@"


        //            <div class='container'>                                    
        //                            <div class='content'>
        //                                <p>Dear {member.UserName},</p>
        //                                <p>We are excited to welcome you to <b>Bugify</b> - our efficient Bug Tracking System.</p>
        //                                <p>Your account has been successfully created with the following details:</p>
        //                                <div class='info'>
        //                                    <p><b>Role:</b> {roleName}</p>
        //                                    <p><b>Assigned Project:</b> {projectName}</p>
        //                                    <p><b>Username:</b> {member.UserName}</p>
        //                                    <p><b>Email:</b> {member.Email}</p>
        //                                   <p><b>Temporary Password:</b> {tempPassword}</p>
        //                                </div>
        //                                <p>You can log in using the above credentials and change your password after logging in.</p>
        //                                <p>If you have any questions, feel free to contact our support team.</p>
        //                                <p>We look forward to your contributions!</p>
        //                            </div>
        //                            <div class='footer'>
        //                                &copy; {DateTime.Now.Year} Bugify. All rights reserved.
        //                            </div>
        //                        </div>";


        //            await _emailSender.SendEmailAsync(
        //                member.Email,
        //                subject,
        //                body,
        //                "NewMemberAdded"
        //                );
        //        }

        //        return Ok(res);

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString);
        //        return Json(new { success = false, message = "Unknown error occured" });
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, bool isActive)
        {
            var members = await _dbBug.Users.FindAsync(id);
            if (members == null)
            {
                return Json(new { success = false, message = "Member not found" });
            }

            members.IsActive = isActive;
            await _dbBug.SaveChangesAsync();

            return Json(new { success = true, message = "Member status updated" });
        }

        [HttpPost]
        public IActionResult DeleteMember(int id)
        {
            var user = _dbBug.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            // Check if the user has any audit logs
            if (_dbBug.AuditLogs.Any(a => a.UserId == id))
            {
                return BadRequest(new { success = false, message = "Cannot delete. User has audit logs." });
            }

            // Check if the user is assigned to any project (UserProject table)
            if (_dbBug.UserProjects.Any(up => up.UserId == id))
            {
                return BadRequest(new { success = false, message = "Cannot delete. User is assigned to one or more projects." });
            }

            // Check if the user has any task assignments
            if (_dbBug.TaskAssignments.Any(t => t.AssignedTo == id))
            {
                return BadRequest(new { success = false, message = "Cannot delete. User has assigned tasks." });
            }

            // If all checks pass, delete the user
            _dbBug.Users.Remove(user);
            _dbBug.SaveChanges();

            return Ok(new { success = true, message = "User deleted successfully" });
        }





        [HttpPost]
        public IActionResult DeleteProjectManager(int id)
        {
            var user = _dbBug.Users.Find(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            _dbBug.Users.Remove(user);
            _dbBug.SaveChanges();

            return Ok(new { message = "User deleted successfully" });
        }

        [HttpPost]
        public IActionResult DeleteDeveloper(int id)
        {
            var user = _dbBug.Users.Find(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            _dbBug.Users.Remove(user);
            _dbBug.SaveChanges();

            return Ok(new { message = "User deleted successfully" });
        }

        [HttpPost]
        public IActionResult DeleteTester(int id)
        {
            var user = _dbBug.Users.Find(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            _dbBug.Users.Remove(user);
            _dbBug.SaveChanges();

            return Ok(new { message = "User deleted successfully" });
        }
    }
}
