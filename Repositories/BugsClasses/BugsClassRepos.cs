using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        
        public async Task<List<Bug>> GetAllBugsData()
        {
            return await _dbBug.Bugs
                .Include(b => b.Attachments)
                .Include(b => b.CreatedByNavigation)
                .Include(b => b.Project)
                .ToListAsync();
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
            if(bug.BugId == 0)
            {
                await _dbBug.Bugs .AddAsync(bug);
            }
            else
            {
                var existingBug = await _dbBug.Bugs.FindAsync(bug.BugId);
                if (existingBug != null)
                {
                    existingBug.Title = bug.Title;
                    existingBug.Description = bug.Description;
                    existingBug.Severity = bug.Severity;
                    existingBug.Priority = bug.Priority;
                    existingBug.StatusId = bug.StatusId;
                }
            }

            await _dbBug.SaveChangesAsync();

            if(attachments?.Count > 0)
            {
                string uploadPath = Path.Combine(_env.WebRootPath, "Attachments");
                Directory.CreateDirectory(uploadPath);

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
                        BugId = bug.BugId,
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
