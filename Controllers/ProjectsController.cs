using Microsoft.AspNetCore.Mvc;
using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Bug_Tracking_System.Controllers
{
    public class ProjectsController : BaseController
    {
        private readonly DbBug _dbBug;
        private readonly IProjectsRepos _project;
        private readonly IEmailSenderRepos _emailsender;
        private readonly IAccountRepos _acc;
        private readonly IPermissionHelperRepos _permission;
        private readonly IAuditLogsRepos _auditLogs;

        public ProjectsController(IAccountRepos acc, IAuditLogsRepos auditLogs, IPermissionHelperRepos permission, IEmailSenderRepos emailsender, IProjectsRepos project, DbBug Bug, ISidebarRepos sidebar) : base(sidebar)
        {
            _dbBug = Bug;
            _project = project;
            _emailsender = emailsender;
            _acc = acc;
            _permission = permission;
            _auditLogs = auditLogs;
        }

        public IActionResult SetCurrentProject(int projectId)
        {
            HttpContext.Session.SetInt32("CurrentProjectId", projectId);
            return Redirect(Request.Headers["Referer"].ToString());
        }

        // Public method to get user permission
        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess(action, roleId);
            ViewBag.PermissionType = permissionType;

            return permissionType;
        }

        [HttpGet, ActionName("ProjectList")]
        public async Task<IActionResult> Index()
        {
            string permissionType = GetUserPermission("View Projects");
            if (permissionType
                == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
            {

                
                ViewBag.PageTitle = "Projects List";
                ViewBag.Breadcrumb = "Reports";
                var projects = await _project.GetAllProjects();
                return View(projects);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        } 

        //Add or Edit project
        [HttpGet]
        public async Task<IActionResult> AddOrEditProject(int? id)
        {
            string permissionType = GetUserPermission("Add Project");
            if ( permissionType == "canEdit" || permissionType == "fullAccess")
            {

                ViewBag.PageTitle = id == null ? "Add New Project" : "Edit Project";
                ViewBag.Breadcrumb = "Manage Projects";

                Project project = new Project();
                if (id > 0)
                {
                    project = await _dbBug.Projects.FirstOrDefaultAsync(p => p.ProjectId == id);
                }


                return View(project);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");

            }
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
                int userId = 2018;

                var result = await _project.AddOrEditProject(projects);
                                
                await _auditLogs.AddAuditLogAsync(userId, $"{projects.ProjectName} project has been added successfully by admin.", "Add Project");

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }


        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Fetch the project for logging before deleting
            var project = await _project.GetProjectById(id); // Ensure this method exists in your repo

            if (project == null)
            {
                return Json(new { success = false, message = "Project not found." });
            }

            var success = await _project.DeleteProject(id);
            if (success)
            {
                // Get logged-in userId from session or default to 2018
                int userId = (int)HttpContext.Session.GetInt32("UserId");

                await _auditLogs.AddAuditLogAsync(userId, $"{project.ProjectName} project was deleted by admin.", "Delete Project");

                return Json(new { success = true, message = "Project deleted successfully." });
            }

            return Json(new { success = false, message = "Failed to delete the project." });
        }


        public async Task<IActionResult> UpdateStatus(int projectId, bool Status)
        {
            var success = await _project.UpdateStatus(projectId, Status);
            if ((bool)success)
                return Json(new { success = true, message = "Status updated successfully." });

            return Json(new { success = false, message = "Project not found." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCompletion(int projectId, int completion)
        {
            var project = await _dbBug.Projects.FindAsync(projectId);
            if (project == null)
            {
                return Json(new { success = false, message = "Project not found" });
            }

            project.Completion = completion;
            await _dbBug.SaveChangesAsync();

            int userId = (int)HttpContext.Session.GetInt32("UserId");
            await _auditLogs.AddAuditLogAsync(userId, $"{project.ProjectName} completion updated to {completion}% by admin.", "Update Completion");

            return Json(new { success = true, message = "Completion updated successfully", newCompletion = completion });
        }



        [HttpGet]
        public async Task<IActionResult> UnassignProjectList()
        {

            string permissionType = GetUserPermission("Assign Project");
            if (permissionType == "canEdit" || permissionType == "fullAccess")
            {
                
                ViewBag.Breadcrumb = "Manage Projects";
                ViewBag.PageTitle = "Unassigned Projects";

                var bugs = await _project.GetUnassignedProjects();
                return View(bugs);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AssignProject(int id)
        {

                ViewBag.PageTitle = "Assign Project";
                ViewBag.Breadcrumb = "Manage Projects";

                ViewBag.Developers = await _dbBug.Users
                        .Where(u => u.Role.RoleName == "Developer") // Ensure this filters only developers
                        .Select(u => new
                        {
                            Value = u.UserId, // Ensure UserId is mapped correctly
                            Text = u.UserName  // Ensure FullName or UserName exists in the User model
                        })
                        .ToListAsync();

                var bug = await _project.GetProjectById(id);
                return View(bug);
            
        }

        //[HttpPost]
        //public async Task<IActionResult> AssignProject(int projectId, int developerId)
        //{

        //    int? userId = HttpContext.Session.GetInt32("UserId");

        //    if (userId == null)
        //    {
        //        return Json(new { success = false, message = "User session expired. Please log in again." });
        //    }

        //    var success = await _project.AssignProjectToDeveloper(projectId, developerId, userId.Value);

        //    if (success)
        //    {
        //        var project = await _project.GetProjectById(projectId);
        //        var developer = await _acc.GetUserById(developerId);
        //        var manager = await _acc.GetUserById(userId.Value);

        //        if (project != null && developer != null && manager != null)
        //        {
        //            string emailBody = $@"
        //                <p><b>Project Name:</b> {project.ProjectName}</p>
        //                <p><b>Assigned By:</b> {manager.UserName}</p>
        //                <p><b>Created Date:</b> {project.CreatedDate:yyyy-MM-dd}</p>
        //                <p><b>Description:</b> {project.Description}</p>
        //            ";

        //            await _emailsender.SendEmailAsync(developer.Email, "New Project Assigned - Bugify", emailBody, "AssignProject");
        //        }
        //    }
        //    return Json(new { success = true, message = "Project assigned successfully!" });
        //}

        [HttpPost]
        public async Task<IActionResult> AssignProject(int projectId, List<int> developerIds)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "User session expired. Please log in again." });
            }

            if (developerIds == null || !developerIds.Any())
            {
                return Json(new { success = false, message = "Please select at least one developer." });
            }

            // ✅ Assign project to multiple developers via UserProject table
            foreach (var devId in developerIds)
            {
                await _project.AssignProjectToDeveloper(projectId, devId, userId.Value);
            }

            // ✅ Email Notification to All Developers
            var project = await _project.GetProjectById(projectId);
            var manager = await _acc.GetUserById(userId.Value);

            if (project != null && manager != null)
            {
                string emailBody = $@"
            <p><b>Project Name:</b> {project.ProjectName}</p>
            <p><b>Assigned By:</b> {manager.UserName}</p>
            <p><b>Created Date:</b> {project.CreatedDate:yyyy-MM-dd}</p>
            <p><b>Description:</b> {project.Description}</p>";

                foreach (var devId in developerIds)
                {
                    var developer = await _acc.GetUserById(devId);
                    if (developer != null)
                    {
                        await _emailsender.SendEmailAsync(developer.Email, "New Project Assigned - Bugify", emailBody, "AssignProject");
                    }
                }

                await _auditLogs.AddAuditLogAsync(userId.Value, $"Project '{project.ProjectName}' assigned to developers ({string.Join(", ", developerIds)}).", "Assign Project");

            }

            return Json(new { success = true, message = "Project assigned successfully to selected developers!" });
        }



        [HttpPost]
        public JsonResult DeleteProject(int projectId, bool forceDeactivate = false)
        {
            try
            {
                var project = _dbBug.Projects
                                    .Include(p => p.Bugs)
                                    .FirstOrDefault(p => p.ProjectId == projectId);

                if (project == null)
                {
                    return Json(new { success = false, message = "Project not found!" });
                }

                if (!project.IsActive)
                {
                    return Json(new { success = false, message = "Project is already deactivated!" });
                }

                // Check for active (non-closed) bugs
                bool hasUnsolvedBugs = project.Bugs.Any(b => b.StatusId != 3);

                if (hasUnsolvedBugs && !forceDeactivate)
                {
                    return Json(new
                    {
                        success = false,
                        requiresConfirmation = true,
                        message = "This project has assigned or unsolved bugs. Do you want to deactivate those bugs as well?",
                        confirmRequired = true
                    });
                }

                // Soft delete the project
                project.IsActive = false;

                if (forceDeactivate)
                {
                    // Deactivate bugs as well
                    foreach (var bug in project.Bugs.Where(b => b.StatusId != 3))
                    {
                        bug.IsActive = false;
                    }
                }

                int changes = _dbBug.SaveChanges();

                if (changes > 0)
                {
                    int userId = (int)HttpContext.Session.GetInt32("UserId");
                    _auditLogs.AddAuditLogAsync(userId, $"Project '{project.ProjectName}' was deactivated" +
                        (forceDeactivate ? " along with active bugs." : "."), "Soft Delete Project");
                }

                return Json(new
                {
                    success = true,
                    message = forceDeactivate
                        ? "Project and its active bugs were deactivated successfully."
                        : "Project was deactivated successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }




        [HttpGet]
        public async Task<IActionResult> ProjectDetails(int id)
        {
            string permissionType = GetUserPermission("Manage Project");
            if (permissionType == "canEdit" || permissionType == "fullAccess")
            {
                ViewBag.PageTitle = "Project Details";
                ViewBag.Breadcrumb = "Manage Projects";

                var project = await _project.GetProjectById(id);
                if (project == null)
                {
                    return NotFound();
                }
                return View(project); // Pass the project with assigned developers
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess","Error");
            }
        }



    }
}