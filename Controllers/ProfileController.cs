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

        public ProfileController(DbBug dbBug ,IProfileRepos profile, ISidebarRepos sidebar) : base(sidebar) 
        {
            _profile = profile;
            _dbBug = dbBug;
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
    }
}
