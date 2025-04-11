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
                

            int roleId = HttpContext.Session.GetInt32("UserRoleId") ?? 0;
            int? userId = HttpContext.Session.GetInt32("UserId");

            if ((userId == 0) || roleId == 0)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Load sidebar tabs
            var tabs = _sidebar.GetTabsByRoleIdAsync(roleId).Result;
            ViewBag.SidebarTabs = tabs;

            // Load user's assigned projects
            var assignedProjects = _sidebar.GetAssignedProjects(userId.Value); // implement this in repo
            ViewBag.AssignedProjects = assignedProjects;

            // Set current project
            int? currentProjectId = HttpContext.Session.GetInt32("CurrentProjectId");
            if (currentProjectId == null && assignedProjects.Any())
            {
                currentProjectId = assignedProjects.First().ProjectId;
                HttpContext.Session.SetInt32("CurrentProjectId", currentProjectId.Value);
            }

            ViewBag.CurrentProjectId = currentProjectId;

            base.OnActionExecuting(context);
        }


        //public override void OnActionExecuting(ActionExecutingContext context)
        //{
        //    int roleId = HttpContext.Session.GetInt32("UserRoleId") ?? 0;

        //    //Fetch session values
        //    int? userId = HttpContext.Session.GetInt32("UserId");

        //    // ✅ Ensure at least one valid login session exists
        //    if ((userId == 0) || roleId == 0)
        //    {
        //        context.Result = new RedirectToActionResult("Login", "Account", null);
        //        return;
        //    }

        //    var tabs = _sidebar.GetTabsByRoleIdAsync(roleId).Result; // Sync for simplicity
        //    ViewBag.SidebarTabs = tabs;

        //    base.OnActionExecuting(context);
        //}
    }
}