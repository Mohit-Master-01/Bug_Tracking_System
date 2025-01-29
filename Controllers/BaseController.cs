using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bug_Tracking_System.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ISidebarRepos _sidebar;
        public BaseController(ISidebarRepos sidebar)
        {
            _sidebar = sidebar;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //int roleId = (int)HttpContext.Session.GetInt32("UserRoleId");
            int roleId = 4;
            var tabs = _sidebar.GetTabsByRoleIdAsync(roleId).Result; // Sync for simplicity
            ViewBag.SidebarTabs = tabs;
            base.OnActionExecuting(context);
        }
    }
}