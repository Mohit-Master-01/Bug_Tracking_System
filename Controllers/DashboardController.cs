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
            if(roleId == 4)
            {
                ViewBag.PageTitle = "Admin's Dashboard";
            }
            else if(roleId == 3)
            {
                ViewBag.PageTitle = "Tester's Dashboard";
            }
            else if(roleId == 2)
            {
                ViewBag.PageTitle = "Developer's Dashboard";
            }
            else
            {
                ViewBag.PageTitle = "Project Manager's Dashboard";
            }

            return View();
        }
    }
}
