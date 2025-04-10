using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bug_Tracking_System.Controllers
{
    public class ErrorController : Controller
    {

        [HttpGet]
        public IActionResult UnauthorisedAccess()
        {
            return View();
        }
    }
}
    