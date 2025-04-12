using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Bug_Tracking_System.Controllers
{
    public class AuditLogsController : BaseController
    {
        private readonly DbBug _dbBug;
        private IAuditLogsRepos _audit;

        public AuditLogsController(DbBug dbBug, IAuditLogsRepos audit, ISidebarRepos sidebar) : base(sidebar)
        {
            _dbBug = dbBug;
            _audit = audit;
        }

        // Developer / Tester / Project Manager - View Own Logs
        public async Task<IActionResult> UserLogs()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var logs = await _audit.GetUserLogsAsync(userId.Value);
            return View(logs);
        }

        public async Task<IActionResult> AllLogs()
        {           

            int? userId = HttpContext.Session.GetInt32("UserId");
            int? userRoleId = HttpContext.Session.GetInt32("UserRoleId");

            ViewBag.Breadcrumb = "Audit Logs";
            ViewBag.PageTitle = (userRoleId == 4) ? "All Logs" : "My Logs";

            if (userId == null || userRoleId == null)
                return RedirectToAction("Login", "Account");

            IEnumerable<AuditLog> logs;

            if (userRoleId == 4) // ✅ Admin 
            {
                logs = await _audit.GetAllLogsAsync();
            }
            else
            {
                logs = await _audit.GetUserLogsAsync(userId.Value);
            }

            return View(logs);
        }


        // ✅ Add Log Entry (Call this from other controllers)
        public async Task<IActionResult> LogAction(string action,string module)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _audit.AddAuditLogAsync(userId, action,module);
            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> ClearLogs([FromBody] ClearLogsRequest request)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "User session expired. Please log in again." });

            var user = await _dbBug.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return Json(new { success = false, message = "User not found." });

            // ✅ Verify password using BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.ConfirmPassword, user.PasswordHash);
            if (!isPasswordValid)
            {
                await _audit.AddAuditLogAsync(user.UserId, $"Failed attempt to clear audit logs due to incorrect password.", "ClearLogs");
                return Json(new { success = false, message = "Incorrect password. Logs were not cleared." });
            }

            // ✅ Clear Audit Logs
            _dbBug.AuditLogs.RemoveRange(_dbBug.AuditLogs);
            await _dbBug.SaveChangesAsync();

            // ✅ Add audit log for the action
            await _audit.AddAuditLogAsync(user.UserId, "Audit logs cleared successfully.", "ClearLogs");

            return Json(new { success = true, message = "Audit logs cleared successfully!" });
        }


        // New Request Model (remove RecaptchaRequest)
        public class ClearLogsRequest
        {
            public string ConfirmPassword { get; set; }
        }


    }
        
}
