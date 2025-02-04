using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bug_Tracking_System.Controllers
{
    public class BugsController : BaseController
    {

        public BugsController(ISidebarRepos sidebar) : base(sidebar) 
        {
            
        }

        [ActionName("BugList")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
