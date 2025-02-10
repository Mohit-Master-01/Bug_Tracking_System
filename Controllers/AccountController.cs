using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;

namespace Bug_Tracking_System.Controllers
{
    public class AccountController : BaseController 
    {

        private readonly IAccountRepos _acc;
        private readonly ILogger<AccountController> _logger;
        private readonly DbBug _dbBug;
        private readonly IEmailSenderRepos _emailSender;
        private readonly ILoginRepos _login;
        private readonly IMemoryCache _memoryCache;

        public AccountController(IAccountRepos acc,ILogger<AccountController> logger, DbBug dbBug, IEmailSenderRepos emailSender, ILoginRepos login,IMemoryCache memoryCache, ISidebarRepos sidebar) : base(sidebar)
        {
            _acc = acc;
            _logger = logger;
            _dbBug = dbBug;
            _emailSender = emailSender;
            _login = login;
            _memoryCache = memoryCache;
        }

        public async Task<IActionResult> Index()
        {
            Console.WriteLine(HttpContext.Session.GetString("Cred"));
            return View();
        }

        [HttpGet, ActionName("Registration")]
        public async Task<IActionResult> AddUserRegister()
        {
            //var roles = await _acc.GetRoles();
            //ViewBag.Roles = roles;

            return View();
        }

        [HttpPost, ActionName("Registration")]
        public async Task<IActionResult> AddUserRegister(User users, IFormFile? ImageFile, int RoleId)
        {
            try
            {
                if (await _acc.IsUsernameExist(users.UserName))
                {
                    return Json(new { success = false, message = $"{users.UserName} already exists." });
                }

                if (await _acc.IsEmailExist(users.Email))
                {
                    return Json(new { success = false, message = $"{users.Email} already exists" });
                }

                // Automatically assign Admin Role (RoleId = 4)
                users.RoleId = 4; // Admin Role
                users.IsAdmin = true; // Mark as Admin


                //TempData["UserEmail"] = user.Email;
                HttpContext.Session.SetString("UserEmail", users.Email);

                //Generate Default Profile Image
                if (ImageFile == null)
                {
                    users.ProfileImage = _acc.GenerateDefaultProfileImage(users.UserName);
                }

                // Get session value
                var email = HttpContext.Session.GetString("UserEmail");
                if (!string.IsNullOrEmpty(email))
                {
                    Console.WriteLine($"User Email: {email}");
                }

                return Json(await _acc.AddUserRegister(users, ImageFile));
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> OtpCheck()
        {
            return View();
        }

        [HttpPost]
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
                    await _emailSender.SendEmailAsync(user.Email, "OTP Verification!!", user.Otp,"Registration");
                    await _dbBug.SaveChangesAsync();
                    return Json(new { success = true, message = "OTP sent successfully" });
                }
                return Json(new { success = false, message = "User not found" });
            }
            return Json(new { success = false, message = "Email not found" });
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            var model = new LoginModel();
            if(Request.Cookies.TryGetValue("RememberMe_Email", out string Emailvalue))
            {
                model.EmailOrUsername = Emailvalue;
                model.RememberMe = true;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel login)
        {
            string RedirectTo = "Dashboard";

            const int maxAttempts = 5; //Maximum allowed attempts
            const int lockoutDurationSeconds = 300; //Lockout duration in seconds

            //Define cache keys for tracking attempts and lockout status
            var attemptKey = $"LoginAttempts_{login.EmailOrUsername}";
            var lockoutKey = $"Lockout_{login.EmailOrUsername}";

            //Check if the user is locked out
            if(_memoryCache.TryGetValue(lockoutKey, out DateTime lockoutEndTime) && lockoutEndTime > DateTime.Now)
            {
                var remainingTime = (int)(lockoutEndTime - DateTime.Now).TotalSeconds;
                return Json(new { success = false, message = $"Account is locked. Try again in {remainingTime} seconds." });
            }

            //Authenticate the user
            var result = await _login.AuthenticateUser(login.EmailOrUsername, login.Password);

            //Set session if login was successful
            if (((dynamic)result).success)
            {
                //Successful login
                HttpContext.Session.SetString("Cred", login.EmailOrUsername);

                if(login.RememberMe)
                {
                    var options = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7), // Cookie expiration time
                        HttpOnly = true,                      // Secure the cookie
                        Secure = true                         // Use HTTPS
                    };

                    Response.Cookies.Append("RememberMe_Email", login.EmailOrUsername, options);
                    Response.Cookies.Append("RememberMe_Password", login.Password, options);

                }
                else
                {
                    Response.Cookies.Delete("RememberMe_Email");
                    Response.Cookies.Delete("RememberMe_Password");
                }

                //Fetch user details and set session variables
                string email = await _acc.fetchEmail(login.EmailOrUsername);
                HttpContext.Session.SetString("UserEmail", email);

                var data = await _acc.GetUserDataByEmail(email);
                int id = data.UserId;
                HttpContext.Session.SetInt32("UserId", id);
                HttpContext.Session.SetInt32("UserRoleId", (int)data.RoleId);

                //Determine redirection based on user verification
                if(data.RoleId == 4)
                {
                    if(await _acc.IsVerified(login.EmailOrUsername))
                    {
                        if(data.RoleId == 4)
                        {
                            RedirectTo = "Dashboard";
                        }
                    }
                    else
                    {
                        RedirectTo = "OtpCheck";
                    }
                }

                if(RedirectTo == "OtpCheck")
                {
                    var user = await _dbBug.Users.FirstOrDefaultAsync(u => u.Email == email);
                    if(user != null)
                    {
                        user.Otp = _emailSender.GenerateOtp();
                        user.OtpExpiry = DateTime.Now.AddMinutes(5);
                        await _emailSender.SendEmailAsync(user.Email, "OTP Verification", user.Otp, "Registration");
                        await _dbBug.SaveChangesAsync();
                    }
                }

                //Reset login attempts after successful login
                _memoryCache.Remove(attemptKey);

                return Ok(new { success = true, message = "You are successfully logged in" });
            }

            else
            {
                // If not successful, return the message from result
                return Json(new { success = false, message = ((dynamic)result).message });
            }

        }

        [HttpGet]
        public async Task<IActionResult> ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(User users)
        {
            HttpContext.Session.SetString("ForgotPassEmail", users.Email);
            var res = await _login.TokenSenderViaEmail(users.Email);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            //validate the token
            if(string.IsNullOrEmpty(token) || !_memoryCache.TryGetValue(token, out object tokenDate))
            {
                return Json(new { success = false, message = "Token is Invalid or Expired, please send another link" });

            }
            return View();
        }

        [HttpPost,ActionName("ResetPassword")]
        public async Task<IActionResult> ResetPasswordAction(string PasswordHash)
        {
            string user = HttpContext.Session.GetString("ForgotPassEmail");

            if (string.IsNullOrEmpty(user))
            {
                _logger.LogWarning("Session expired or 'ForgotPassEmail' not set.");
                return Json(new { success = false, message = "Session expired. Please request a new password reset." });
            }

            var res = await _login.ResetPassword(user, PasswordHash);
            if (((dynamic)res).success)
            {
                HttpContext.Session.Remove("ForgotPassEmail"); // Clear session after successful reset
            }
            return Ok(res);
        }

    }
}