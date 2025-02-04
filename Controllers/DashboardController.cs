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
            return View();
        }
    }
}
