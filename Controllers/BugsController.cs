using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace Bug_Tracking_System.Controllers
{
    public class BugsController : BaseController
    {

        private readonly DbBug _dbBug;
        private readonly IBugRepos _bug;
        
        public BugsController(DbBug dbBug, IBugRepos bug, ISidebarRepos sidebar) : base(sidebar) 
        {
            _dbBug = dbBug;
            _bug = bug;
        }

        [HttpGet]
        public async Task<IActionResult> BugList(int? page)
        {
            try
            {
                int pageSize = 4;
                int pageNumber = page ?? 1;

                ViewBag.PageTitle = "Bug List";
                ViewBag.Breadcrumb = "Manage Bugs";

                var bugs = await _bug?.GetAllBugsData(pageNumber,pageSize);

                if (bugs == null || !bugs.Any())
                {
                    return View(new List<Bug>()); // Return empty list if null
                }

                ViewBag.StatusList = new SelectList(await _dbBug.BugStatuses.ToListAsync(), "StatusId", "StatusName");


                return View(bugs);
            }
            catch (Exception ex)
            {
                // Log error (ensure you use ILogger)
                Console.WriteLine($"Error fetching bug list: {ex.Message}");
                return View("Error"); // Redirect to an error page
            }
        }


        [HttpGet]
        public async Task<IActionResult> BugDetails(int id)
        {
            ViewBag.PageTitle = "Bug Details";
            ViewBag.Breadcrumb = "Manage Bugs";

            var bug = await _bug.GetBugById(id);

            if(bug == null)
            {
                return NotFound();
            }

            ViewBag.StatusList = new SelectList(await _dbBug.BugStatuses.ToListAsync(), "StatusId", "StatusName");

            return View(bug);
        }

        [HttpGet]
        public async Task<IActionResult> SaveBug(int? id)
        {
            ViewBag.Breadcrumb = "Manage Bugs";

            if (id == null)
            {
                ViewBag.PageTitle = "Add a Bug";
            }
            else
            {
                ViewBag.PageTitle = "Edit Bug";

            }

            Bug bugs = new Bug();

            if(id > 0)
            {
                bugs = await _dbBug.Bugs.FirstOrDefaultAsync(b => b.BugId == id);
            }

            // Fetch the bug along with attachments
            var bug = await _dbBug.Bugs
                .Include(b => b.Attachments)  // Load attachments
                .FirstOrDefaultAsync(b => b.BugId == id);

            // Retrieve UserId and UserName from session
            int? userId = HttpContext.Session.GetInt32("UserId");
            var username = HttpContext.Session.GetString("UserName");

            if (userId == null)
            {
                return RedirectToAction("Login"); // Redirect to login if session is null
            }

            // Check if session exists, otherwise set a default value
            ViewBag.TestedBy = !string.IsNullOrEmpty(username) ? username : "Unknown User";

            //Define priority levels
            ViewBag.Priority = new List<string> { "Highest", "High", "Medium", "Low", "Lowest" };

            //Define priority levels
            ViewBag.Severity = new List<string> { "Critical", "Major", "Minor", "Low" };

            // Fetch projects dynamically
            ViewBag.Projects = new SelectList(await _dbBug.Projects
                                            .Select(p => new { p.ProjectId, p.ProjectName })
                                            .ToListAsync(), "ProjectId", "ProjectName");

            // Fetch status dynamically
            ViewBag.Status = new SelectList(await _dbBug.BugStatuses
                                            .Select(s => new { s.StatusId, s.StatusName })
                                            .ToListAsync(), "StatusId", "StatusName");
            return View(bugs);
        }

        [HttpPost]
        public async Task<IActionResult> SaveBug(Bug bugs, List<IFormFile> attachments)
        {
            // Retrieve UserId from session to insert into database
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Json(new { success = false, message = "User session expired. Please log in again." });
            }

            // Assign the logged-in user's ID to the CreatedBy field (Foreign Key)
            bugs.CreatedBy = userId.Value;

            var success = await _bug.SaveBug(bugs, attachments);
            return RedirectToAction("BugList");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int bugId, int statusId)
        {
            var bug = await _dbBug.Bugs.FindAsync(bugId);
            if (bug == null)
            {
                return Json(new { success = false, message = "Bug not found" });
            }

            bug.StatusId = statusId;
            await _dbBug.SaveChangesAsync();

            var updatedStatus = await _dbBug.BugStatuses
                .Where(s => s.StatusId == statusId)
                .Select(s => s.StatusName)
                .FirstOrDefaultAsync();

            return Json(new { success = true, message = "Status updated successfully", newStatus = updatedStatus });
        }

        
        [HttpPost]
        public async Task<IActionResult> Delete(int bugId)
        {
            var success = await _bug.DeleteBug(bugId);
            return Json(new { success, message = success ? "Bug deleted successfully!" : "Failed to delete bug." });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            var attachment = await _dbBug.Attachments.FindAsync(id);

            if (attachment == null)
            {
                return Json(new { success = false, message = "Attachment not found." });
            }

            _dbBug.Attachments.Remove(attachment);
            await _dbBug.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> UnassignBugList(int? page)
        {
            int pageSize = 4;
            int pageNumber = page ?? 1;

            ViewBag.Breadcrumb = "Manage Bugs";
            ViewBag.PageTitle = "Unassigned Bugs";

            var bugs = await _bug.GetUnassignedBugs(pageNumber, pageSize);
            return View(bugs);
        }

        [HttpGet]
        public async Task<IActionResult> AssignBug(int id)
        {
            ViewBag.PageTitle = "Assign Bug";
            ViewBag.Breadcrumb = "Manage Bugs";

            ViewBag.Developers = await _dbBug.Users
                    .Where(u => u.Role.RoleName == "Developer") // Ensure this filters only developers
                    .Select(u => new 
                    {
                        Value = u.UserId, // Ensure UserId is mapped correctly
                        Text = u.UserName  // Ensure FullName or UserName exists in the User model
                    })
                    .ToListAsync();

            var bug = await _bug.GetBugById(id);
            return View(bug);
        }

        [HttpPost]
        public async Task<IActionResult> AssignBug(int bugId, int developerId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if(userId == null)
            {
                return Json(new { success = false, message = "User session expired. Please log in again." });
            }

            var success = await _bug.AssignBugToDeveloper(bugId, developerId, userId.Value);
            return Json(new { success, message = success ? "Bug assigned successfully!" : "Failed to assign bug." });
        }

    }
}
