using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace Bug_Tracking_System.Controllers
{
    public class MembersController : BaseController
    {
        private readonly DbBug _dbBug;
        private readonly IMembersRepos _member;
        private readonly IAccountRepos _acc;
        private readonly IEmailSenderRepos _emailSender;

        public MembersController(DbBug dbBug, IMembersRepos member, IEmailSenderRepos emailSender , IAccountRepos acc , ISidebarRepos sidebar) : base(sidebar) 
        {
            _dbBug = dbBug;
            _member = member;
            _acc = acc;
            _emailSender = emailSender;
        }

        [ActionName("MembersList")]
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 4; // Number of records per page
            int pageNumber = page ?? 1; // Default to page 1

            ViewBag.PageTitle = "Members List";
            ViewBag.Breadcrumb = "Reports";
            var members = await _member.GetAllMembers(pageNumber, pageSize);
            return View(members);
        }

        [HttpGet]
        public async Task<IActionResult> ProjectManagersList(int? page)
        {
            int pageSize = 4;
            int pageNumber = page ?? 1;
            ViewBag.PageTitle = "Project Managers List";
            ViewBag.Breadcrumb = "Reports";
            var ProjectManagers = await _member.GetAllProjectManagers(pageNumber, pageSize);
            return View(ProjectManagers);
        }

        [HttpGet]
        public async Task<IActionResult> DevelopersList(int? page)
        {
            int pageSize = 4;
            int pageNumber = page ?? 1;
            ViewBag.PageTitle = "Developers List";
            ViewBag.Breadcrumb = "Reports";

            var developers = await _member.GetAllDevelopers(pageNumber, pageSize);

            if (developers == null || !developers.Any())
            {
                ViewBag.Message = "No developers found.";
            }

            return View(developers);
        }

        [HttpGet]
        public async Task<IActionResult> TestersList(int? page)
        {
            int pageSize = 4;
            int pageNumber = page ?? 1;
            ViewBag.PageTitle = "Tester List";
            ViewBag.Breadcrumb = "Reports";
            var Testers = await _member.GetAllTesters(pageNumber, pageSize);

            if (Testers == null || !Testers.Any())
            {
                ViewBag.Message = "No developers found.";
            }
            return View(Testers);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            User member = _dbBug.Users.FirstOrDefault(m => m.UserId == id);
            ViewBag.Member = member;
            return View();
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
            if(id > 0)
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

        public async Task<IActionResult> SaveMember(User member, IFormFile? ImageFile)
        {
            try
            {
                if(member.UserId > 0)
                {
                    User resData = _member.checkExistence(member.UserName, member.Email, member.UserId);
                    if(resData != null)
                    {
                        if (resData.Email == member.Email)
                        {
                            return Json(new { success = false, message = "Email already exists" });
                        }
                        if (resData.UserName == member.UserName)
                        {
                            return Json(new { success = false, message = "Username already exists" });
                        }
                    }
                }
                else
                {
                    if (await _acc.IsUsernameExist(member.UserName))
                    {
                        return Json(new { success = false, message = "Username already exists - edit" });
                    }
                    if (await _acc.IsEmailExist(member.Email))
                    {
                        return Json(new { success = false, message = "Email already exists - edit" });
                    }
                }
                string username = member.UserName;
                //// **Generate a Temporary Password for New User**
                //string tempPassword = GenerateRandomPassword();
                ////member.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
                //string password = tempPassword;
                string email = member.Email;
                int id = member.UserId;

                int ResId = (int)HttpContext.Session.GetInt32("UserId");

                var res = await _member.SaveMember(member, ImageFile);

                if (((dynamic)res).success)
                {
                    // ✅ **Fetch Role Name**
                    string roleName = await _dbBug.Roles
                        .Where(r => r.RoleId == member.RoleId)
                        .Select(r => r.RoleName)
                        .FirstOrDefaultAsync() ?? "Not Assigned";

                    // ✅ **Fetch Project Name**
                    string projectName = await _dbBug.Projects
                        .Where(p => p.ProjectId == member.ProjectId)
                        .Select(p => p.ProjectName)
                        .FirstOrDefaultAsync() ?? "Not Assigned";

                    // ✅ **Get Temporary Password**
                    var responseObj = (dynamic)res;
                    string tempPassword = responseObj.tempPassword ?? "Not Available";

                    string subject = "Welcome to Bugify - Your Account is Ready!";
                    string body = $@"


                    <div class='container'>                                    
                                    <div class='content'>
                                        <p>Dear {member.UserName},</p>
                                        <p>We are excited to welcome you to <b>Bugify</b> - our efficient Bug Tracking System.</p>
                                        <p>Your account has been successfully created with the following details:</p>
                                        <div class='info'>
                                            <p><b>Role:</b> {roleName}</p>
                                            <p><b>Assigned Project:</b> {projectName}</p>
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

                    
                    await _emailSender.SendEmailAsync(
                        member.Email,
                        subject,
                        body,
                        "NewMemberAdded"
                        );
                }

                return Ok(res);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString);
                return Json(new { success = false, message = "Unknown error occured" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, bool isActive)
        {
            var members = await _dbBug.Users.FindAsync(id);
            if(members == null)
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
