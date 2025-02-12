using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bug_Tracking_System.Controllers
{
    public class DashboardController : BaseController
    {

        public DashboardController(ISidebarRepos sidebar) : base(sidebar) 
        {
            
        }

        [ActionName("Dashboard")]
        public IActionResult Index()
        {
            int? roleId = HttpContext.Session.GetInt32("UserRoleId");

            ViewBag.Breadcrumb = "Dashboard";
            ViewBag.PageTitle = (roleId == 4) ? "Admin Dashboard" : "Member Dashboard";

            return View();
        }
    }
}
