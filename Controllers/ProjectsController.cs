using Microsoft.AspNetCore.Mvc;
using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bug_Tracking_System.Controllers
{
    public class ProjectsController : BaseController
    {
        private readonly DbBug _dbBug;
        private readonly IProjectsRepos _project;

        public ProjectsController(IProjectsRepos project, DbBug Bug, ISidebarRepos sidebar) : base(sidebar)
        {
            _dbBug = Bug;
            _project = project;
        }

        [HttpGet, ActionName("ProjectList")]
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 4; // Number of records per page
            int pageNumber = page ?? 1; // Default to page 1

            ViewBag.PageTitle = "Projects List";
            ViewBag.Breadcrumb = "Reports";
            var projects = await _project.GetAllProjects(pageNumber, pageSize);
            return View(projects);
        } 

        //Add or Edit project
        [HttpGet]
        public async Task<IActionResult> AddOrEditProject(int? id)
        {
            ViewBag.PageTitle = id == null ? "Add New Project" : "Edit Project";
            ViewBag.Breadcrumb = "Manage Projects";

            Project project = new Project();
            if(id > 0)
            {
                project = await _dbBug.Projects.FirstOrDefaultAsync(p => p.ProjectId == id);
            }

            return View(project);
        }

        [HttpPost,ActionName("AddOrEditProject")]
        public async Task<IActionResult> AddOrEditProject(Project projects)
        {
            // Ensure CreatedBy is always set before saving
            if (projects.CreatedBy == 0)
            {
                projects.CreatedBy = 2018; // Default to admin or logged-in user
            }

            // Remove CreatedByNavigation from validation
            ModelState.Remove("CreatedByNavigation");

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    );

                return Json(new { success = false, message = "Validation failed", errors });
            }

            try
            {
                var result = await _project.AddOrEditProject(projects);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }


        [HttpPost,ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {            
            var success = await _project.DeleteProject(id);
            if(success)
                return Json(new { success = true, message = "Project deleted successfully." });

            return Json(new { success = false, message = "Project not found." });
        }

        public async Task<IActionResult> UpdateStatus(int projectId, bool Status)
        {
            var success = await _project.UpdateStatus(projectId, Status);
            if ((bool)success)
                return Json(new { success = true, message = "Status updated successfully." });

            return Json(new { success = false, message = "Project not found." });
        }
    }
}
