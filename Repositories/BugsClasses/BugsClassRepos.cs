using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;


namespace Bug_Tracking_System.Repositories.BugsClasses
{
    public class BugsClassRepos : IBugRepos
    {
        private readonly DbBug _dbBug;
        private readonly IWebHostEnvironment _env;


        public BugsClassRepos(DbBug dbBug, IWebHostEnvironment env)
        {
            _dbBug = dbBug;       
            _env = env;
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
                .Include(b => b.Status)  // Ensure Status is loaded
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
            return await _dbBug.Bugs
                .Include(b => b.CreatedByNavigation)
                .Include(b => b.Project)
                .Include(b => b.Attachments)
                .FirstOrDefaultAsync(b => b.BugId == bugId);
        }

        public async Task<bool> SaveBug(Bug bug, List<IFormFile> attachments)
        {
            var existingBug = await _dbBug.Bugs
                .Include(b => b.Attachments) // Ensure attachments are included
                .FirstOrDefaultAsync(b => b.BugId == bug.BugId);

            if (existingBug != null)
            {
                // Update existing bug details
                existingBug.Title = bug.Title;
                existingBug.Description = bug.Description;
                existingBug.Severity = bug.Severity;
                existingBug.Priority = bug.Priority;
                existingBug.StatusId = bug.StatusId;

                // Track changes properly
                _dbBug.Entry(existingBug).State = EntityState.Modified;
            }
            else
            {
                _dbBug.Bugs.Add(bug);
            }

            await _dbBug.SaveChangesAsync();

            // ✅ Attachments Handling (Edit & Add)
            if (attachments?.Count > 0)
            {
                string uploadPath = Path.Combine(_env.WebRootPath, "Attachments");
                Directory.CreateDirectory(uploadPath);

                // ✅ Remove old attachments (Only if editing)
                if (existingBug != null)
                {
                    var oldAttachments = _dbBug.Attachments.Where(a => a.BugId == existingBug.BugId).ToList();
                    foreach (var oldAttachment in oldAttachments)
                    {
                        string oldFilePath = Path.Combine(_env.WebRootPath, oldAttachment.FilePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    _dbBug.Attachments.RemoveRange(oldAttachments);
                }

                // ✅ Add new attachments
                foreach (var file in attachments)
                {
                    string fileExtension = Path.GetExtension(file.FileName).ToLower();
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx" };

                    if (!allowedExtensions.Contains(fileExtension) || file.Length > 5 * 1024 * 1024) // 5MB limit
                    {
                        continue; // Skip invalid files
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                    string filePath = Path.Combine(uploadPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var bugAttachment = new Attachment
                    {
                        BugId = existingBug != null ? existingBug.BugId : bug.BugId, // Ensure correct BugId
                        FilePath = "/Attachments/" + uniqueFileName
                    };

                    await _dbBug.Attachments.AddAsync(bugAttachment);
                }

                await _dbBug.SaveChangesAsync();
            }

            return true;
        }

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
