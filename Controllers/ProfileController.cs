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

        public ProfileController(DbBug dbBug, IAccountRepos acc ,IProfileRepos profile, IEmailSenderRepos emailSender, ISidebarRepos sidebar) : base(sidebar) 
        {
            _profile = profile;
            _dbBug = dbBug;
            _emailSender = emailSender;
            _acc = acc;
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
                return RedirectToAction("Login", "Account");
            }

            users.UserId = userId.Value; //Ensure correct user Id is used 

            await _profile.EditProfile(users, ImageFile);
            return RedirectToAction("Profile");
        }

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