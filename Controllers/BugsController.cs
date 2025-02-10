using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
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

        
        public  async Task<IActionResult> BugList(int? page)
        {
            int pageSize = 4; // Number of records per page
            int pageNumber = page ?? 1; // Default to page 1

            ViewBag.PageTitle = "Bug List";
            ViewBag.Breadcrumb = "Manage";
            var bugs = await _bug.GetAllBugsData();
            return View(bugs);
        }

        [HttpGet]
        public async Task<IActionResult> BugDetails(int id)
        {

            var bug = await _bug.GetBugById(id);
            if(bug == null)
            {
                return NotFound();
            }
            return View(bug);
        }

        [HttpGet]
        public async Task<IActionResult> SaveBug(int? id)
        {
            return View();
        }

        public async Task<IActionResult> SaveBug(Bug bugs, List<IFormFile> attachments)
        {
            var success = await _bug.SaveBug(bugs, attachments);
            return Json(new { success, message = success ? "Bug saved successfully!" : "Error occurred!" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int bugId, int statusId)
        {
            var success = await _bug.UpdateBugStatus(bugId, statusId);
            return Json(new { success, message = success ? "Bug status updated!" : "Failed to update status." });
        }

        // 🟢 Delete Bug
        [HttpPost]
        public async Task<IActionResult> Delete(int bugId)
        {
            var success = await _bug.DeleteBug(bugId);
            return Json(new { success, message = success ? "Bug deleted successfully!" : "Failed to delete bug." });
        }
    }
}
