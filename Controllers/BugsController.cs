using Bug_Tracking_System.DTOs;
using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories;
using Bug_Tracking_System.Repositories.Interfaces;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office.CoverPageProps;
using DocumentFormat.OpenXml.Wordprocessing;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Composition;
using System.Data;
using System.IO.Compression;
using System.Net.Mail;

namespace Bug_Tracking_System.Controllers
{
    public class BugsController : BaseController
    {

        private readonly DbBug _dbBug;
        private readonly IBugRepos _bug;
        private readonly IAccountRepos _acc;
        private readonly IEmailSenderRepos _emailSender;
        private readonly INotificationRepos _notification;
        private readonly IPermissionHelperRepos _permission;
        private readonly IExportRepos _export;
        private readonly IImportRepos _import;
        private readonly IAuditLogsRepos _auditLog;
        private readonly GoogleCalendarService _calendarService;
        private readonly IWebHostEnvironment _env;
        public BugsController(IEmailSenderRepos emailSender, IWebHostEnvironment webHostEnvironment, IAuditLogsRepos auditLog, IImportRepos import, IExportRepos export, IAccountRepos acc, DbBug dbBug, IBugRepos bug, INotificationRepos notification, IPermissionHelperRepos permission, GoogleCalendarService calendarService, ISidebarRepos sidebar) : base(sidebar)
        {
            _dbBug = dbBug;
            _bug = bug;
            _acc = acc;
            _emailSender = emailSender;
            _notification = notification;
            _permission = permission;
            _export = export;
            _import = import;
            _auditLog = auditLog;
            _calendarService = calendarService;
            _env = webHostEnvironment;
        }

        // Public method to get user permission
        public string GetUserPermission(string action)
        {
            int roleId = HttpContext.Session.GetInt32("UserRoleId").Value;
            string permissionType = _permission.HasAccess(action, roleId);
            ViewBag.PermissionType = permissionType;

            return permissionType;
        }

        [HttpGet]
        public async Task<IActionResult> BugList()
        {
            try
            {
                string permissionType = GetUserPermission("View Bugs");
                if (permissionType == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
                {
                    ViewBag.PageTitle = "Bug List";
                    ViewBag.Breadcrumb = "Manage Bugs";

                    int? currentProjectId = HttpContext.Session.GetInt32("CurrentProjectId");

                    int? roleId = HttpContext.Session.GetInt32("UserRoleId");

                    if ((currentProjectId == null || currentProjectId == 0) && roleId != 4 && roleId != 3) // Only validate projectId if not admin
                    {
                        TempData["Error"] = "Please select a project first.";
                        return RedirectToAction("Index", "Home");
                    }

                    List<Bug> bugs;

                    if (roleId == 4 || roleId == 3) // Admin
                    {
                        // Fetch all unassigned bugs for admin
                        bugs = await _bug.GetAllBugs(); // You can create this method if not existing
                    }
                    else
                    {
                        // For project users
                        bugs = await _bug.GetBugsByProject((int)currentProjectId);
                    }

                    ViewBag.StatusList = new SelectList(await _dbBug.BugStatuses.ToListAsync(), "StatusId", "StatusName");

                    return View(bugs);
                }
                else
                {
                    return RedirectToAction("UnauthorisedAccess", "Error");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching bug list: {ex.Message}");
                return View("Error");
            }
        }

      
        //[HttpGet]
        //public async Task<IActionResult> BugList(int? page)
        //{
        //    try
        //    {
        //        string permissionType = GetUserPermission("View Bugs");
        //        if (permissionType
        //            == "canView" || permissionType == "canEdit" || permissionType == "fullAccess")
        //        {
        //            int pageSize = 4;
        //            int pageNumber = page ?? 1;

        //            ViewBag.PageTitle = "Bug List";
        //            ViewBag.Breadcrumb = "Manage Bugs";

        //            var bugs = await _bug?.GetAllBugsData(pageNumber, pageSize);

        //            if (bugs == null || !bugs.Any())
        //            {
        //                return View(new List<Bug>()); // Return empty list if null
        //            }

        //            ViewBag.StatusList = new SelectList(await _dbBug.BugStatuses.ToListAsync(), "StatusId", "StatusName");


        //            return View(bugs);
        //        }
        //        else
        //        {
        //            return RedirectToAction("UnauthorisedAccess", "Error");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log error (ensure you use ILogger)
        //        Console.WriteLine($"Error fetching bug list: {ex.Message}");
        //        return View("Error"); // Redirect to an error page
        //    }
        //}

        public async Task<IActionResult> ExportBugList()
        {
            var bugs = await _dbBug.Bugs
                            .Include(b => b.Attachments)  // Include attachments for images
                            .Include(b => b.CreatedByNavigation) // Ensure CreatedByNavigation is loaded
                            .Include(b => b.Project)  // Ensure Project is loaded
                            .Where(b => b.Status.StatusId != 1)  // Ensure Status is loaded
                            .Select(b => new Bug
                            {
                                BugId = b.BugId,
                                Title = b.Title,
                                Description = b.Description, // Ensure description is fetched
                                CreatedDate = b.CreatedDate,
                                Priority = b.Priority,
                                Severity = b.Severity,
                                Status = b.Status,
                                CreatedByNavigation = b.CreatedByNavigation,
                                Project = b.Project,
                                Attachments = b.Attachments
                            })
                            .ToListAsync();


            var dataTable = new DataTable("BugListReports");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Bug Id"),
                new DataColumn("Title"),
                new DataColumn("Description"),
                new DataColumn("Severity"),
                new DataColumn("Priority"),
                new DataColumn("Created Date"),
                new DataColumn("Created By"),
                new DataColumn("Status")
            });

            foreach (var bug in bugs)
            {
                dataTable.Rows.Add(bug.BugId, bug.Title, bug.Description, bug.Severity, bug.Priority, bug.CreatedDate, bug.CreatedByNavigation.UserName, bug.Status.StatusName);
            }

            var fileBytes = _export.ExportToExcel(dataTable, "BugListReports");

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BugListReports.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> AddBugToGoogleCalendar(int bugId)
        {
            try
            {
                var bug = await _bug.GetBugById(bugId);
                if (bug == null)
                    return Json(new { success = false, message = "Bug not found" });

                // 1. Load credentials
                string credentialsPath = Path.Combine(_env.ContentRootPath, "credentials.json");
                UserCredential credential;

                using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    string tokenPath = Path.Combine(_env.ContentRootPath, "token.json");
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        new[] { CalendarService.Scope.Calendar },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(tokenPath, true));
                }

                // 2. Create the Calendar Service
                var calendarService = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Bug Tracking Calendar",
                });

                // 3. Prepare the Event
                Event newEvent = new Event()
                {
                    Summary = bug.Title,
                    Description = bug.Description,
                    Start = new EventDateTime()
                    {
                        DateTime = DateTime.Now,
                        TimeZone = "Asia/Kolkata"
                    },
                    End = new EventDateTime()
                    {
                        DateTime = DateTime.Now.AddHours(1),
                        TimeZone = "Asia/Kolkata"
                    }
                };

                // 4. Insert the Event
                var request = calendarService.Events.Insert(newEvent, "primary");
                await request.ExecuteAsync();

                return Json(new { success = true, message = "Bug event added to Google Calendar" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> BugDetails(int id)
        {
            string permissionType = GetUserPermission("View Bugs");
            if (permissionType == "canEdit" || permissionType == "fullAccess")
            {
                ViewBag.PageTitle = "Bug Details";
                ViewBag.Breadcrumb = "Manage Bugs";

                var bug = await _bug.GetBugById(id);

                if (bug == null)
                {
                    return NotFound();
                }

               

                ViewBag.StatusList = new SelectList(await _dbBug.BugStatuses.ToListAsync(), "StatusId", "StatusName");

                return View(bug);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }



        [HttpGet]
        public async Task<IActionResult> SaveBug(int? id)
        {
            string permissionType = GetUserPermission("Report Bug");
            if (permissionType == "canEdit" || permissionType == "fullAccess")
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

                if (id > 0)
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
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
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

            await _auditLog.AddAuditLogAsync(userId.Value, $"Bug '{bugs.Title}' created", "Report Bug");


            return RedirectToAction("BugList");
        }

        public IActionResult DownloadSampleFile()
        {
            var fileBytes = _import.GenerateSampleBugExcel(false);

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SampleFile.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> ImportBugsWithImages(IFormFile excelFile, IFormFile imageZip)
        {
            var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            if (excelFile == null || imageZip == null)
                return BadRequest("Both Excel file and ZIP of images are required.");

            if (!imageZip.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                return Json(new { success = false, message = "Only .zip files are allowed for images." });

            if (!excelFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return Json(new { success = false, message = "Only .xlsx files are allowed for data." });

            var imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BugAttachments");
            Directory.CreateDirectory(imageFolderPath);

            // ✅ Extract images from ZIP
            using (var zipStream = new MemoryStream())
            {
                await imageZip.CopyToAsync(zipStream);
                zipStream.Seek(0, SeekOrigin.Begin);

                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                {
                    foreach (var entry in archive.Entries)
                    {
                        string extension = Path.GetExtension(entry.Name).ToLower();
                        if (!allowedExtensions.Contains(extension))
                            return Json(new { success = false, message = "ZIP contains invalid file format: " + entry.Name });

                        string imagePath = Path.Combine(imageFolderPath, entry.Name);
                        if (!System.IO.File.Exists(imagePath))
                            entry.ExtractToFile(imagePath);
                    }
                }
            }

            // ✅ Process Excel File
            try
            {
                var importResult = await _import.BugsExcelImport(excelFile); // You already have this method
                string fileBase64 = Convert.ToBase64String(importResult);

                return Json(new
                {
                    success = true,
                    message = "Bugs imported successfully!",
                    fileData = fileBase64,
                    fileName = "BugImportResult.xlsx"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error occurred: " + ex.Message });
            }
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

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                await _auditLog.AddAuditLogAsync(userId.Value, $"Bug ID {bugId} status changed to {updatedStatus}", "BugController");
            }

            bool projectUpdated = false;
            int newCompletion = 0;

            // 👉 Recalculate completion if status is "Fixed"
            if (updatedStatus == "Fixed" && bug.ProjectId.HasValue)
            {
                int projectId = bug.ProjectId.Value;

                var totalBugs = await _dbBug.Bugs
                    .CountAsync(b => b.ProjectId == projectId);

                var closedBugs = await _dbBug.Bugs
                    .CountAsync(b => b.ProjectId == projectId && b.Status.StatusName == "Fixed");

                newCompletion = totalBugs > 0 ? (int)((closedBugs * 100.0) / totalBugs) : 0;

                var project = await _dbBug.Projects.FindAsync(projectId);
                if (project != null)
                {
                    project.Completion = newCompletion;
                    await _dbBug.SaveChangesAsync();
                    projectUpdated = true;

                    await _auditLog.AddAuditLogAsync(userId ?? 0, $"Project '{project.ProjectName}' completion auto-updated to {newCompletion}%", "BugController");
                }
            }

            return Json(new
            {
                success = true,
                message = "Status updated successfully",
                newStatus = updatedStatus,
                projectUpdated,
                newCompletion = projectUpdated ? newCompletion : (int?)null
            });
        }


        //[HttpPost]
        //public async Task<IActionResult> UpdateStatus(int bugId, int statusId)
        //{
        //    var bug = await _dbBug.Bugs.FindAsync(bugId);
        //    if (bug == null)
        //    {
        //        return Json(new { success = false, message = "Bug not found" });
        //    }

        //    bug.StatusId = statusId;
        //    await _dbBug.SaveChangesAsync();

        //    var updatedStatus = await _dbBug.BugStatuses
        //        .Where(s => s.StatusId == statusId)
        //        .Select(s => s.StatusName)
        //        .FirstOrDefaultAsync();

        //    int? userId = HttpContext.Session.GetInt32("UserId");
        //    if (userId != null)
        //    {
        //        await _auditLog.AddAuditLogAsync(userId.Value, $"Bug ID {bugId} status changed to {updatedStatus}", "BugController");
        //    }

        //    return Json(new { success = true, message = "Status updated successfully", newStatus = updatedStatus });
        //}


        [HttpPost]
        public async Task<IActionResult> Delete(int bugId)
        {
            var success = await _bug.DeleteBug(bugId);

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                await _auditLog.AddAuditLogAsync(userId.Value, $"Bug ID {bugId} deleted (via service)", "Delete Bug");
            }

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
        public async Task<IActionResult> UnassignBugList()
        {
            string permissionType = GetUserPermission("Assign Bug");

            if (permissionType == "canEdit" || permissionType == "fullAccess")
            {
                ViewBag.Breadcrumb = "Manage Bugs";
                ViewBag.PageTitle = "Unassigned Bugs";

                int? currentProjectId = HttpContext.Session.GetInt32("CurrentProjectId");
                int? roleId = HttpContext.Session.GetInt32("UserRoleId");

                if ((currentProjectId == null || currentProjectId == 0) && roleId != 4) // Only validate projectId if not admin
                {
                    TempData["Error"] = "Please select a project first.";
                    return RedirectToAction("Index", "Home");
                }

                List<Bug> bugs;

                if (roleId == 4) // Admin
                {
                    // Fetch all unassigned bugs for admin
                    bugs = await _bug.GetAllUnassignedBugs(); // You can create this method if not existing
                }
                else
                {
                    // For project users
                    bugs = await _bug.GetUnassignedBugsByProject((int)currentProjectId);
                }

                return View(bugs);
            }
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }



        //[HttpGet]
        //public async Task<IActionResult> UnassignBugList(int? page)
        //{
        //    string permissionType = GetUserPermission("Assign Bug");
        //    if (permissionType == "canEdit" || permissionType == "fullAccess")
        //    {
        //        int pageSize = 4;
        //        int pageNumber = page ?? 1;

        //        ViewBag.Breadcrumb = "Manage Bugs";
        //        ViewBag.PageTitle = "Unassigned Bugs";

        //        var bugs = await _bug.GetUnassignedBugs(pageNumber, pageSize);
        //        return View(bugs);
        //    }
        //    else
        //    {
        //        return RedirectToAction("UnauthorisedAccess", "Error");
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> AssignBug(int id)
        {
            string permissionType = GetUserPermission("Assign Bug");
            if (permissionType == "canEdit" || permissionType == "fullAccess")
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
            else
            {
                return RedirectToAction("UnauthorisedAccess", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssignBug(int bugId, int developerId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Json(new { success = false, message = "User session expired. Please log in again." });
            }

            var bugid = await _bug.GetBugById(bugId);
            var developerid = await _acc.GetUserById(developerId);

            if (bugid == null || developerid == null)
            {
                await _auditLog.AddAuditLogAsync(userId.Value, $"Attempted to assign Bug ID {bugId} to Developer ID {developerId}, but bug or developer was not found.", "Assign Bug");

                return Json(new { success = false, message = "Invalid bug or developer selected." });
            }

            // 🚨 Validate Project Match
            if (bugid.ProjectId != developerid.ProjectId)
            {
                await _auditLog.AddAuditLogAsync(userId.Value, $"Attempted to assign Bug ID {bugId} to Developer ID {developerId}, but developer not in same project.", "Assign Bug");

                return Json(new { success = false, message = "This developer is not assigned to the same project as this bug. Assignment denied." });
            }

            var success = await _bug.AssignBugToDeveloper(bugId, developerId, userId.Value);

            if (success)
            {
                var bug = await _bug.GetBugById(bugId);
                var developer = await _acc.GetUserById(developerId);
                var manager = await _acc.GetUserById(userId.Value);

                if (bug != null && developer != null && manager != null)
                {
                    string emailBody = $@"
                <p><b>Bug Title:</b> {bug.Title}</p>
                <p><b>Assigned By:</b> {manager.UserName}</p>
                <p><b>Reported Date:</b> {bug.CreatedDate:yyyy-MM-dd}</p>
                <p><b>Description:</b> {bug.Description}</p>
            ";

                    await _emailSender.SendEmailAsync(developer.Email, "New Bug Assigned - Bugify", emailBody, "AssignBug");
                }

                await _notification.AddNotification(userId.Value, 1, "You have been assigned a new bug!", bugId, "Bug");


                await _auditLog.AddAuditLogAsync(userId.Value, $"Bug ID {bugId} assigned to Developer ID {developerId}", "Assign Bug");

            }


            return Json(new { success = true, message = "Bug assigned successfully!" });
        }

        [HttpPost]
        public JsonResult DeleteBug(int bugId)
        {
            try
            {
                var bug = _dbBug.Bugs.Find(bugId);
                if (bug == null)
                {
                    return Json(new { success = false, message = "Bug not found!" });
                }

                // Remove related records in TaskAssignment
                var taskAssignments = _dbBug.TaskAssignments.Where(t => t.BugId == bugId);
                _dbBug.TaskAssignments.RemoveRange(taskAssignments);

                // Remove related records in Attachment
                var attachments = _dbBug.Attachments.Where(a => a.BugId == bugId);
                _dbBug.Attachments.RemoveRange(attachments);

                // Set BugId to NULL in Users table instead of deleting users
                var users = _dbBug.Users.Where(u => u.BugId == bugId);
                foreach (var user in users)
                {
                    user.BugId = null;
                }

                // Finally, remove the bug
                _dbBug.Bugs.Remove(bug);

                // Save all changes in a single transaction
                _dbBug.SaveChanges();

                int? userId = HttpContext.Session.GetInt32("UserId");
                if (userId != null)
                {
                    _ = _auditLog.AddAuditLogAsync(userId.Value, $"Bug ID {bugId} deleted with cascading records", "Delete Bug");
                }

                return Json(new { success = true, message = "Bug deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server error: " + ex.Message });
            }
        }



    }
}
