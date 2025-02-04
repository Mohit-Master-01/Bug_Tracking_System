using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bug_Tracking_System.Controllers
{
    public class ProfileController : BaseController
    {
        private readonly IProfileRepos _profile;


        public ProfileController(IProfileRepos profile, ISidebarRepos sidebar) : base(sidebar) 
        {
            _profile = profile;
        }

        [ActionName("Profile")]
        public async Task<IActionResult> Index()
        {

            var user = await _profile.GetAllUsersData();
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
    }
}
