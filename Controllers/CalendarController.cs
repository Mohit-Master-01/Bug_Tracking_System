using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Bug_Tracking_System.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Bug_Tracking_System.Controllers
{
    public class CalendarController : BaseController
    {
        private readonly DbBug _dbBug;

        public CalendarController(DbBug dbBug, ISidebarRepos sidebar) : base(sidebar) 
        {
            _dbBug = dbBug;
        }

        public IActionResult BugsCalendar()
        {
            ViewBag.PageTitle = "Bug Calendar";
            ViewBag.Breadcrumb = "View Bugs";

            var projects = _dbBug.Projects.ToList();
            ViewBag.ProjectList = new SelectList(projects, "ProjectId", "ProjectName");

            return View();
        }

        [HttpGet]
        public JsonResult GetBugEvents()
        {
            int? currentProjectId = HttpContext.Session.GetInt32("CurrentProjectId");
            int? roleId = HttpContext.Session.GetInt32("UserRoleId");

            IQueryable<Bug> bugsQuery;

            if (roleId == 4) // Admin
            {
                bugsQuery = _dbBug.Bugs;
            }
            else
            {
                if (currentProjectId == null || currentProjectId == 0)
                {
                    return Json(new { error = "Please select a project first." });
                }

                bugsQuery = _dbBug.Bugs.Where(b => b.ProjectId == currentProjectId);
            }

            var bugEvents = bugsQuery.Select(b => new BugCalendarDTO
            {
                BugId = b.BugId,
                Title = b.Title,
                Start = b.CreatedDate.ToString("yyyy-MM-dd"),
                Url = Url.Action("BugDetails", "Bugs", new { id = b.BugId }),
                Severity = b.Severity,
                ProjectId = b.ProjectId,
                Status = b.Status.StatusName
            }).ToList();

            return Json(bugEvents);
        }

        //public async Task<IActionResult> BugsCalendar()
        //{
        //    ViewBag.PageTitle = "Bug Calendar";
        //    ViewBag.Breadcrumb = "View Bugs";

        //    int? currentProjectId = HttpContext.Session.GetInt32("CurrentProjectId");
        //    int? roleId = HttpContext.Session.GetInt32("UserRoleId");

        //    if ((currentProjectId == null || currentProjectId == 0) && roleId != 4)
        //    {
        //        TempData["Error"] = "Please select a project first.";
        //        return RedirectToAction("Index", "Home");
        //    }

        //    List<Project> projects;

        //    if (roleId == 4)
        //    {
        //        projects = await _dbBug.Projects.ToListAsync(); // Admin sees all
        //    }
        //    else
        //    {
        //        projects = await _dbBug.Projects
        //            .Where(p => p.ProjectId == currentProjectId)
        //            .ToListAsync(); // Member sees only assigned project
        //    }

        //    ViewBag.ProjectList = new SelectList(projects, "ProjectId", "ProjectName");
        //    return View();
        //}

        //[HttpGet]
        //public async Task<JsonResult> GetBugEvents()
        //{
        //    int? currentProjectId = HttpContext.Session.GetInt32("CurrentProjectId");
        //    int? roleId = HttpContext.Session.GetInt32("UserRoleId");

        //    List<Bug> bugs;

        //    if (roleId == 4)
        //    {
        //        // Admin: get all bugs
        //        bugs = await _dbBug.Bugs.ToListAsync();
        //    }
        //    else
        //    {
        //        if (currentProjectId == null || currentProjectId == 0)
        //        {
        //            return Json(new { success = false, message = "No project selected" });
        //        }

        //        // Members: get only project-specific bugs
        //        bugs = await _dbBug.Bugs
        //                .Where(b => b.ProjectId == currentProjectId)
        //                .ToListAsync();
        //    }

        //    var events = bugs.Select(b => new
        //    {
        //        title = b.Title,
        //        start = b.CreatedDate.ToString("yyyy-MM-dd"),
        //        url = Url.Action("BugDetails", "Bugs", new { id = b.BugId }),
        //        severity = b.Severity,
        //        projectId = b.ProjectId,
        //        status = b.Status
        //    });

        //    return Json(events);

        //}
    }
}
