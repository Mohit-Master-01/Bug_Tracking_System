using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Bug_Tracking_System.DTOs;
using Bug_Tracking_System.Models;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.InkML;

namespace Bug_Tracking_System.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly IAccountRepos _user;
        private readonly IBugRepos _bug;
        private readonly IProjectsRepos _project;
        private readonly DbBug _dbBug;
        private readonly IDashboardRepos _dashboard;

        public DashboardController(IAccountRepos user, IDashboardRepos dashboard, DbBug dbBug, IBugRepos bug, IProjectsRepos project, ISidebarRepos sidebar) : base(sidebar) 
        {
            _user = user;
            _bug = bug;
            _project = project;
            _dbBug = dbBug;
            _dashboard = dashboard;
        }

        //[ActionName("Dashboard")]
        public IActionResult Index()
        {
            int? roleId = HttpContext.Session.GetInt32("UserRoleId");

            ViewBag.Breadcrumb = "Dashboard";
            if (roleId == 4)
            {
                ViewBag.PageTitle = "Admin's Dashboard";
            }
            else if (roleId == 3)
            {
                ViewBag.PageTitle = "Tester's Dashboard";
            }
            else if (roleId == 2)
            {
                ViewBag.PageTitle = "Developer's Dashboard";
            }
            else
            {
                ViewBag.PageTitle = "Project Manager's Dashboard";
            }

            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId").Value;
            var role = HttpContext.Session.GetInt32("UserRoleId").Value;

            ViewBag.Breadcrumb = "Dashboard";
            if (role == 4)
            {
                ViewBag.PageTitle = "Admin's Dashboard";
            }
            else if (role == 3)
            {
                ViewBag.PageTitle = "Tester's Dashboard";
            }
            else if (role == 2)
            {
                ViewBag.PageTitle = "Developer's Dashboard";
            }
            else
            {
                ViewBag.PageTitle = "Project Manager's Dashboard";
            }

            var dashboardData = await _dashboard.GetDashboardData(userId, role);
            return View(dashboardData);
        }

        public IActionResult Documentation()
        {

            ViewBag.Breadcrumb = "Help & Documentation";            
            ViewBag.PageTitle = "Help & Documentation";

                return View();
        }


    }
}
