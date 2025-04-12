using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bug_Tracking_System.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly IProfileRepos _profile;
        private readonly DbBug _dbBug;
        private readonly IEmailSenderRepos _emailSender;
        private readonly IAccountRepos _acc;
        private readonly IMembersRepos _member;
        private readonly IPermissionHelperRepos _permission;
        private readonly IProjectsRepos _projects;

        public ProfileController(DbBug dbBug, IAccountRepos acc ,IProfileRepos profile, IProjectsRepos projects, IPermissionHelperRepos permission, IEmailSenderRepos emailSender, IMembersRepos member, ISidebarRepos sidebar) : base(sidebar) 
        {
            _profile = profile;
            _dbBug = dbBug;
            _emailSender = emailSender;
            _acc = acc;
            _member = member;
            _permission = permission;
            _projects = projects;
        }

        // Public method to get user permission
        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess(action, roleId);
            ViewBag.PermissionType = permissionType;
            return permissionType;
        }

        [ActionName("Profile")]
        public async Task<IActionResult> Index()
        {

            //Get the logged-in user Id from session
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? roleId = HttpContext.Session.GetInt32("UserRoleId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect if user is not logged in
            }

            User users = new User();

            ViewBag.Breadcrumb = "Profile";

            ViewBag.PageTitle = (roleId == 4) ? "Admin Profile" : "Member Profile";


            //Fetch user data dynamically
            var user = await _profile.GetAllUsersData(userId.Value);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect if user is not logged in
            }

            return View(user);

            //var user = await _profile.GetAllUsersData();
            //if (user == null)
            //{
            //    user = new Models.User();
            //}
            //return View(user);
        }

        [HttpGet]
        public async  Task<IActionResult> MyTeam()
        {
            try
            {
                string permissionType = GetUserPermission("View Developers");

                if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
                {
                    ViewBag.PageTitle = "My Team";
                    ViewBag.Breadcrumb = "Profile";

                    int? currentProjectId = HttpContext.Session.GetInt32("CurrentProjectId");
                    int? roleId = HttpContext.Session.GetInt32("UserRoleId");

                    if ((currentProjectId == null || currentProjectId == 0) && roleId != 4)
                    {
                        TempData["Error"] = "Please select a project first.";
                        return RedirectToAction("Index", "Home");
                    }

                    List<User> members;

                    if (roleId == 4) // Admin
                    {
                        members = await _member.GetAllMembers();
                    }
                    else
                    {
                        members = await _member.GetAllMembersByProject((int)currentProjectId);
                    }

                    // Stats for dashboard/cards
                    ViewBag.TotalMembers = members.Count;
                    ViewBag.ActiveMembers = members.Count(u => u.IsActive == true);
                    ViewBag.InactiveMembers = members.Count(u => u.IsActive == false);
                    ViewBag.JoinedThisMonth = members.Count(u =>
                                                        u.CreatedDate.HasValue &&
                                                        u.CreatedDate.Value.Month == DateTime.Now.Month &&
                                                        u.CreatedDate.Value.Year == DateTime.Now.Year);

                    return View(members);
                }
                else
                {
                    return RedirectToAction("UnauthorisedAccess", "Error");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching project managers list: {ex.Message}");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyProjects()
        {
            try
            {
                string permissionType = GetUserPermission("View Developers");

                if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
                {
                    ViewBag.PageTitle = "My Projects";
                    ViewBag.Breadcrumb = "Profile";

                    int? userId = HttpContext.Session.GetInt32("UserId");
                    int? roleId = HttpContext.Session.GetInt32("UserRoleId");

                    if (!userId.HasValue || !roleId.HasValue)
                    {
                        TempData["Error"] = "Session expired. Please login again.";
                        return RedirectToAction("Login", "Account");
                    }

                    List<Project> projects;

                    if (roleId.Value == 4) // Admin
                    {
                        projects = await _projects.GetAllProjects(); // Get all projects
                    }
                    else if (roleId.Value == 1 || roleId.Value == 2) // PM or Developer
                    {
                        projects = await _projects.GetProjectByUser(userId.Value); // Assigned projects
                    }
                    else
                    {
                        TempData["Error"] = "Access not permitted.";
                        return RedirectToAction("UnauthorisedAccess", "Error");
                    }

                    // Stats
                    ViewBag.TotalProjects = projects.Count;
                    ViewBag.ActiveProjects = projects.Count(p => p.Status == "In Progress" || p.Status == "Active");
                    ViewBag.CompletedProjects = projects.Count(p => p.Completion == 100);
                    ViewBag.ProjectsThisMonth = projects.Count(p =>
                                                    p.CreatedDate.HasValue &&
                                                    p.CreatedDate.Value.Month == DateTime.Now.Month &&
                                                    p.CreatedDate.Value.Year == DateTime.Now.Year);

                    return View(projects);
                }
                else
                {
                    return RedirectToAction("UnauthorisedAccess", "Error");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MyProjects(): {ex.Message}");
                return View("Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> GenerateDefaultProfile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Json(new { success = false, message = "Session expired. Please login again." });
            }

            var user = await _dbBug.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            string relativePath = _acc.GenerateDefaultProfileImage(user.UserName);

            // Remove old profile image if exists
            if (!string.IsNullOrEmpty(user.ProfileImage))
            {
                string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfileImage.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            user.ProfileImage = relativePath;
            await _dbBug.SaveChangesAsync();

            return Json(new { success = true, imagePath = relativePath });
        }


        [HttpGet]
        public async Task<IActionResult> EditProfile(int id)
        {
            int? roleId = HttpContext.Session.GetInt32("UserRoleId");

            ViewBag.Breadcrumb = "Profile";

            ViewBag.PageTitle = (roleId == 4) ? "Edit Admin Profile" : "Edit Member Profile";

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            User user = new User();
            if(id > 0)
            {
                user = await _dbBug.Users.FirstOrDefaultAsync(u => u.UserId == id);
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(User users, IFormFile? ImageFile)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Json(new { success = false, message = "User session expired. Please login again." });
            }

            users.UserId = userId.Value; // Ensure correct user ID is used 

            try
            {
                var updatedUser = await _profile.EditProfile(users, ImageFile);

                if (updatedUser == null)
                {
                    return Json(new { success = false, message = "User not found!" });
                }

                // ✅ Update Session Values after Profile Update
                HttpContext.Session.SetString("UserName", updatedUser.UserName ?? "");
                string userImagePath = string.IsNullOrEmpty(updatedUser.ProfileImage) ? "/assets/default-user.png" : updatedUser.ProfileImage;
                HttpContext.Session.SetString("UserImage", userImagePath);

                return Json(new { success = true, message = "Profile updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error while updating profile. " + ex.Message });
            }
        }


        //[HttpPost]
        //public async Task<IActionResult> EditProfile(User users, IFormFile? ImageFile)
        //{
        //    int? userId = HttpContext.Session.GetInt32("UserId");

        //    if (userId == null)
        //    {
        //        return Json(new { success = false, message = "User session expired. Please login again." });
        //    }

        //    users.UserId = userId.Value; // Ensure correct user ID is used 

        //    try
        //    {
        //        await _profile.EditProfile(users, ImageFile);
        //        // Update session after successful image change


        //        return Json(new { success = true, message = "Profile updated successfully!" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Error while updating profile. " + ex.Message });
        //    }
        //}


        //[HttpPost]
        //public async Task<IActionResult> EditProfile(User users, IFormFile? ImageFile)
        //{
        //    int? userId = HttpContext.Session.GetInt32("UserId");

        //    if (userId == null)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    users.UserId = userId.Value; //Ensure correct user Id is used 

        //    await _profile.EditProfile(users, ImageFile);
        //    return RedirectToAction("Profile");
        //}

        [HttpPost]
        public async Task<IActionResult> UpdateEmailVerification([FromBody] User users)
        {
            try
            {
                if (users == null || string.IsNullOrEmpty(users.Email))
                {
                    return Json(new { success = false, message = "Invalid user or email is empty!" });
                }

                // Store Email in Session
                HttpContext.Session.SetString("UserEmail", users.Email);
                var email = HttpContext.Session.GetString("UserEmail");

                if (!string.IsNullOrEmpty(email))
                {
                    Console.WriteLine($"User Email Stored in Session: {email}");
                }

                // Call OTP generation and email sending service
                //var result = await _profile.UpdateEmailVerification(users);
                return Json(await _profile.UpdateEmailVerification(users));


                // Ensure the session value is set correctly before redirecting
                //return Json(new { success = true, message = "OTP Sent!", redirectUrl = "/Profile/ProfileOTP" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet, ActionName("ProfileOTP")]
        public async Task<IActionResult> OtpCheck()
        {
            return View();
        }

        [HttpPost, ActionName("ProfileOTP")]
        public async Task<IActionResult> OtpCheck(User users)
        {
            try
            {
                if (await _acc.IsEmailExist(users.Email))
                {
                    if (await _acc.OtpVerification(users.Otp))
                    {
                        await _acc.updateStatus(users.Email);
                        return Json(new { success = true, message = "OTP Verified Successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "OTP Verification Failed :(" });

                    }
                }

                return Json(new { success = true, message = "Email not found!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString);
                return Json(new { success = false, message = "Unknown error occured" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ResendOtp()
        {
            string email = HttpContext.Session.GetString("UserEmail");
            if (email != null)
            {
                var user = await _dbBug.Users.FirstOrDefaultAsync(x => x.Email == email);
                if (user != null)
                {
                    user.Otp = _emailSender.GenerateOtp();
                    user.OtpExpiry = DateTime.Now.AddMinutes(5);
                    await _emailSender.SendEmailAsync(user.Email, "OTP Verification!!", user.Otp, "Registration");
                    await _dbBug.SaveChangesAsync();
                    return Json(new { success = true, message = "OTP sent successfully" });
                }
                return Json(new { success = false, message = "User not found" });
            }
            return Json(new { success = false, message = "Email not found" });
        }

    }
}