using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Printing;
using X.PagedList;
using X.PagedList.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.Build.Evaluation;
using DocumentFormat.OpenXml.Spreadsheet;


namespace Bug_Tracking_System.Repositories.BugsClasses
{
    public class BugsClassRepos : IBugRepos
    {
        private readonly DbBug _dbBug;
        private readonly IWebHostEnvironment _env;
        private readonly INotificationRepos _notification;
        private readonly IAuditLogsRepos _auditLogs;


        public BugsClassRepos(DbBug dbBug, IAuditLogsRepos auditLogs, INotificationRepos notification,  IWebHostEnvironment env)
        {
            _dbBug = dbBug;       
            _env = env;
            _notification = notification;
            _auditLogs = auditLogs;
        }


        public async Task<bool> AssignBugToDeveloper(int bugId, int developerId, int assignedBy)
        {
            try
            {
                // 🔍 Fetch Bug along with Project and TaskAssignments in one go
                var bug = await _dbBug.Bugs
                    .Include(b => b.TaskAssignments)
                    .FirstOrDefaultAsync(b => b.BugId == bugId);

                if (bug == null) return false;

                // 🔍 Check Developer Exists
                var developer = await _dbBug.Users
                    .FirstOrDefaultAsync(u => u.UserId == developerId);

                var assigner = await _dbBug.Users.FirstOrDefaultAsync(u => u.UserId == assignedBy);


                if (developer == null) return false;

                // 🔥 Create Assignment
                var assignment = new TaskAssignment
                {
                    BugId = bugId,
                    AssignedTo = developerId,
                    AssignedBy = assignedBy,
                    ProjectManagerId = assignedBy,
                    ProjectId = bug.ProjectId,
                    AssignedDate = DateTime.UtcNow,
                    CompletionDate = null
                };

                // ✅ Attach assignment via navigation property
                bug.TaskAssignments.Add(assignment);

                // ✅ Update Bug Status to "Assigned" (StatusId = 2)
                bug.StatusId = await _dbBug.BugStatuses
                    .Where(s => s.StatusId == 2)
                    .Select(s => s.StatusId)
                    .FirstOrDefaultAsync();

                // 🛠 (Optional) Update developer's latest BugId if needed
                developer.BugId = bugId;

                // 🚀 Save all together
                await _dbBug.SaveChangesAsync();

                // 🔔 Send Notification (Post Save)
                await _notification.AddNotification(
                    userId: developerId,
                    type: 1,
                    message: "A new bug has been assigned to you!",
                    relatedId: bugId,
                moduleType: "Bug"
                );

                await _auditLogs.AddAuditLogAsync(assignedBy, $"Bug ID {bugId} assigned to Developer ID {developerId}", "Assign Bug");


                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AssignBugToDeveloper: {ex.Message}");
                return false;
            }
        }



        public async Task<bool> DeleteBug(int bugId)
        {
            var bug = await _dbBug.Bugs.FindAsync(bugId);
            if (bug != null)
            {
                _dbBug.Bugs.Remove(bug);
                return await _dbBug.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<List<Bug>> GetBugsByProject(int projectId)
        {
            if (_dbBug == null)
                throw new InvalidOperationException("Database context (_dbBug) is not initialized.");

            var bugs = await _dbBug.Bugs
                .Include(b => b.Attachments)
                .Include(b => b.CreatedByNavigation)
                .Include(b => b.Project)
                .Where(b => b.ProjectId == projectId && b.IsActive == true)
                .OrderByDescending(b => b.CreatedDate)
                .Select(b => new Bug
                {
                    BugId = b.BugId,
                    Title = b.Title,
                    Description = b.Description,
                    CreatedDate = b.CreatedDate,
                    Priority = b.Priority,
                    Severity = b.Severity,
                    Status = b.Status,
                    CreatedByNavigation = b.CreatedByNavigation,
                    Project = b.Project,
                    Attachments = b.Attachments
                })
                .ToListAsync();

            return bugs;
        }


        public async Task<Bug> GetBugById(int bugId)
        {
            var bug = await _dbBug.Bugs
                .Include(b => b.Attachments)
                .Include(b => b.CreatedByNavigation)
                .Include(b => b.Project)
                .Include(b => b.Status)
                .FirstOrDefaultAsync(b => b.BugId == bugId);

            if (bug != null)
            {
                // Get task assignment details for this bug (if exists)
                var taskAssignment = await (
                    from task in _dbBug.TaskAssignments
                    join assignedTo in _dbBug.Users on task.AssignedTo equals assignedTo.UserId
                    join projectManager in _dbBug.Users on task.ProjectManagerId equals projectManager.UserId
                    where task.BugId == bugId
                    select new
                    {
                        AssignedTo = assignedTo,
                        ProjectManager = projectManager,
                        CompletionDate = task.CompletionDate
                    }
                ).FirstOrDefaultAsync();

                // Attach TaskAssignment details to bug's TaskAssignments collection if exists
                if (taskAssignment != null)
                {
                    bug.TaskAssignments = new List<TaskAssignment>
            {
                new TaskAssignment
                {
                    AssignedToNavigation = taskAssignment.AssignedTo,
                    ProjectManager = taskAssignment.ProjectManager,
                    CompletionDate = taskAssignment.CompletionDate ?? null
                }
            };
                }

                return bug;
            }

            return null;
        }


        public async Task<List<User>> GetDevelopers()
        {
            return await _dbBug.Users
                        .Where(u => u.RoleId == 2) // ✅ Adjust based on your user roles
                        .ToListAsync();
        }

        public async Task<List<Bug>> GetUnassignedBugsByProject(int projectId)
        {
            var bugs = await _dbBug.Bugs
                .Include(b => b.Attachments)
                .Include(b => b.CreatedByNavigation)
                .Include(b => b.Project)
                .Where(b => b.ProjectId == projectId && b.Status.StatusId == 1 && b.IsActive == true)
                .OrderByDescending(b => b.CreatedDate)
                .Select(b => new Bug
                {
                    BugId = b.BugId,
                    Title = b.Title,
                    Description = b.Description,
                    CreatedDate = b.CreatedDate,
                    Priority = b.Priority,
                    Severity = b.Severity,
                    Status = b.Status,
                    CreatedByNavigation = b.CreatedByNavigation,
                    Project = b.Project,
                    Attachments = b.Attachments
                })
                .ToListAsync();

            return bugs;
        }


        public async Task<bool> SaveBug(Bug bug, List<IFormFile> attachments)
        {
            try
            {
                Bug existingBug = await _dbBug.Bugs
                    .Include(b => b.Attachments)
                    .FirstOrDefaultAsync(b => b.BugId == bug.BugId);

                if (existingBug != null)
                {
                    existingBug.Title = bug.Title;
                    existingBug.Description = bug.Description;
                    existingBug.Severity = bug.Severity;
                    existingBug.Priority = bug.Priority;
                    existingBug.StatusId = bug.StatusId;
                    existingBug.CreatedBy = bug.CreatedBy;
                    existingBug.CreatedDate = DateTime.Now;
                    existingBug.IsActive = true;
                }
                else
                {
                    bug.CreatedDate = DateTime.Now;
                    bug.IsActive = true;
                    _dbBug.Bugs.Add(bug);
                }

                await _dbBug.SaveChangesAsync();

                int bugId = existingBug?.BugId ?? bug.BugId;

                if (attachments != null && attachments.Any())
                {
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BugAttachments");
                    Directory.CreateDirectory(uploadPath);

                    List<Attachment> bugAttachments = new List<Attachment>();

                    foreach (var file in attachments)
                    {
                        string fileExtension = Path.GetExtension(file.FileName).ToLower();
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx" };

                        if (!allowedExtensions.Contains(fileExtension) || file.Length > 5 * 1024 * 1024)
                        {
                            continue;
                        }

                        string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        string filePath = Path.Combine(uploadPath, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        bugAttachments.Add(new Attachment
                        {
                            BugId = bugId,
                            FilePath = "/BugAttachments/" + uniqueFileName,
                            UploadedDate = DateTime.UtcNow
                        });
                    }

                    await _dbBug.Attachments.AddRangeAsync(bugAttachments);
                    await _dbBug.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Saving Bug: " + ex.Message);
                return false;
            }
        }


        //public async Task<bool> SaveBug(Bug bug, List<IFormFile> attachments)
        //{
        //    var existingBug = await _dbBug.Bugs
        //        .Include(b => b.Attachments) // Ensure attachments are included
        //        .FirstOrDefaultAsync(b => b.BugId == bug.BugId);

        //    if (existingBug != null)
        //    {
        //        // Update existing bug details
        //        existingBug.Title = bug.Title;
        //        existingBug.Description = bug.Description;
        //        existingBug.Severity = bug.Severity;
        //        existingBug.Priority = bug.Priority;
        //        existingBug.StatusId = bug.StatusId;

        //        // Track changes properly
        //        _dbBug.Entry(existingBug).State = EntityState.Modified;
        //    }
        //    else
        //    {
        //        _dbBug.Bugs.Add(bug);
        //    }

        //    await _dbBug.SaveChangesAsync();

        //    // ✅ Attachments Handling (Edit & Add)
        //    if (attachments?.Count > 0)
        //    {
        //        string uploadPath = Path.Combine(_env.WebRootPath, "Attachments");
        //        Directory.CreateDirectory(uploadPath);

        //        // ✅ Remove old attachments (Only if editing)
        //        if (existingBug != null)
        //        {
        //            var oldAttachments = _dbBug.Attachments.Where(a => a.BugId == existingBug.BugId).ToList();
        //            foreach (var oldAttachment in oldAttachments)
        //            {
        //                string oldFilePath = Path.Combine(_env.WebRootPath, oldAttachment.FilePath.TrimStart('/'));
        //                if (System.IO.File.Exists(oldFilePath))
        //                {
        //                    System.IO.File.Delete(oldFilePath);
        //                }
        //            }
        //            _dbBug.Attachments.RemoveRange(oldAttachments);
        //        }

        //        // ✅ Add new attachments
        //        foreach (var file in attachments)
        //        {
        //            string fileExtension = Path.GetExtension(file.FileName).ToLower();
        //            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx" };

        //            if (!allowedExtensions.Contains(fileExtension) || file.Length > 5 * 1024 * 1024) // 5MB limit
        //            {
        //                continue; // Skip invalid files
        //            }

        //            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
        //            string filePath = Path.Combine(uploadPath, uniqueFileName);

        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await file.CopyToAsync(stream);
        //            }

        //            var bugAttachment = new Attachment
        //            {
        //                BugId = existingBug != null ? existingBug.BugId : bug.BugId, // Ensure correct BugId
        //                FilePath = "/Attachments/" + uniqueFileName
        //            };

        //            await _dbBug.Attachments.AddAsync(bugAttachment);
        //        }

        //        await _dbBug.SaveChangesAsync();
        //    }

        //    return true;
        //}

        public async Task<bool> UpdateBugStatus(int bugId, int statusId)
        {
            var bug = await _dbBug.Bugs.FindAsync(bugId);
            if (bug != null)
            {
                bug.StatusId = statusId;
                return await _dbBug.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<List<Bug>> GetAllUnassignedBugs()
        {
            var bugs = await _dbBug.Bugs
                           .Include(b => b.Attachments)
                           .Include(b => b.CreatedByNavigation)
            .Include(b => b.Project)
                           .Where(b => b.Status.StatusId == 1 && b.IsActive == true)
                           .OrderByDescending(b => b.CreatedDate)
                           .Select(b => new Bug
                           {
                               BugId = b.BugId,
                               Title = b.Title,
                               Description = b.Description,
                               CreatedDate = b.CreatedDate,
                               Priority = b.Priority,
                               Severity = b.Severity,
                               Status = b.Status,
                               CreatedByNavigation = b.CreatedByNavigation,
                               Project = b.Project,
                               Attachments = b.Attachments
                           })
                           .ToListAsync();

            return bugs;
        }

        public async Task<List<Bug>> GetAllBugs()
        {
            if (_dbBug == null)
                throw new InvalidOperationException("Database context (_dbBug) is not initialized.");

            var bugs = await _dbBug.Bugs
                .Include(b => b.Attachments)
                .Include(b => b.CreatedByNavigation)
            .Include(b => b.Project)
            .Where(b => b.IsActive == true)
                .OrderByDescending(b => b.CreatedDate)
                .Select(b => new Bug
                {
                    BugId = b.BugId,
                    Title = b.Title,
                    Description = b.Description,
                    CreatedDate = b.CreatedDate,
                    Priority = b.Priority,
                    Severity = b.Severity,
                    Status = b.Status,
                    CreatedByNavigation = b.CreatedByNavigation,
                    Project = b.Project,
                    Attachments = b.Attachments
                })
                .ToListAsync();

            return bugs;
        }
    }
}
