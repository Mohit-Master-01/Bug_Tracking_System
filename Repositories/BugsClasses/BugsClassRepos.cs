using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Printing;
using X.PagedList;
using X.PagedList.Extensions;


namespace Bug_Tracking_System.Repositories.BugsClasses
{
    public class BugsClassRepos : IBugRepos
    {
        private readonly DbBug _dbBug;
        private readonly IWebHostEnvironment _env;
        private readonly INotificationRepos _notification;


        public BugsClassRepos(DbBug dbBug, INotificationRepos notification,  IWebHostEnvironment env)
        {
            _dbBug = dbBug;       
            _env = env;
            _notification = notification;
        }

        
        public async Task<bool> AssignBugToDeveloper(int bugId, int developerId, int assignedBy)
        {
            var user = await _dbBug.Users.FirstOrDefaultAsync(u => u.UserId == developerId);
            if (user == null) return false;

            var bug = await _dbBug.Bugs.FindAsync(bugId);
            if (bug == null) return false;

            var assignment = new TaskAssignment
            {
                BugId = bugId,
                AssignedTo = developerId,
                AssignedBy = assignedBy,
                ProjectManagerId = assignedBy, // 🔹 Provide a default or correct value
                AssignedDate = DateTime.Now,
                CompletionDate = null, // 🔹 If nullable, set explicitly
                ProjectId = bug.ProjectId // 🔹 Ensure it matches related bug project
            };

            await _dbBug.TaskAssignments.AddAsync(assignment);

            // Update Bug Status
            var assignedStatus = await _dbBug.BugStatuses
                .Where(s => s.StatusId == 2) // Status: Assigned
                .Select(s => s.StatusId)
                .FirstOrDefaultAsync();

            if (assignedStatus > 0)
            {
                bug.StatusId = assignedStatus;
            }

            user.BugId = bugId;

            await _dbBug.SaveChangesAsync();

            await _notification.AddNotification(
            userId: developerId,
            type: 1,
            message: "A new bug has been assigned to you!",
            relatedId: bugId,
            moduleType: "Bug"
            );

            return true;
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
                
        public async Task<IPagedList<Bug>> GetAllBugsData(int pageNumber, int pageSize)
        {
            if (_dbBug == null)
            {
                throw new InvalidOperationException("Database context (_dbBug) is not initialized.");
            }

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

            return bugs.ToPagedList(pageNumber, pageSize);

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
                return new Bug
                {
                    BugId = bug.BugId,
                    Title = bug.Title,
                    Description = bug.Description,
                    CreatedDate = bug.CreatedDate,
                    Priority = bug.Priority,  // Ensure this is not null
                    Severity = bug.Severity,  // Ensure this is not null
                    Status = bug.Status,  // Handle null status
                    CreatedByNavigation = bug.CreatedByNavigation,
                    Project = bug.Project,
                    Attachments = bug.Attachments
                };
            }
            return null;

        }

        public async Task<List<User>> GetDevelopers()
        {
            return await _dbBug.Users
                        .Where(u => u.RoleId == 2) // ✅ Adjust based on your user roles
                        .ToListAsync();
        }

        public async Task<IPagedList<Bug>> GetUnassignedBugs(int pageNumber, int pageSize)
        {
            if (_dbBug == null)
            {
                throw new InvalidOperationException("Database context (_dbBug) is not initialized.");
            }

            var bugs = await _dbBug.Bugs
                .Include(b => b.Attachments)  // Include attachments for images
                .Include(b => b.CreatedByNavigation) // Ensure CreatedByNavigation is loaded
                .Include(b => b.Project)  // Ensure Project is loaded
                .Where(b => b.Status.StatusId == 1 )  // Ensure Status is loaded
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

            return bugs.ToPagedList(pageNumber, pageSize);
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
                }
                else
                {
                    bug.CreatedDate = DateTime.Now;
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
    }
}
