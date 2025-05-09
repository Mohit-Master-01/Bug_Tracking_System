﻿using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bug_Tracking_System.Repositories
{
    public class AuditLogsClassRepos : IAuditLogsRepos
    {
        private readonly DbBug _dbBug;

        public AuditLogsClassRepos(DbBug dbBug)
        {
            _dbBug = dbBug;            
        }

        public async Task AddAuditLogAsync(int userId, string action, string module)
        {
            try
            {
                var log = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    ModuleName = module,
                    ActionDate = DateTime.Now
                };

                await _dbBug.AuditLogs.AddAsync(log);
                await _dbBug.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                // Log database-related issues
                Console.WriteLine($"Database error while adding audit log: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                // Log any other exceptions
                Console.WriteLine($"Unexpected error while adding audit log: {ex.Message}");
            }
        }


        public async Task<IEnumerable<AuditLog>> GetAllLogsAsync()
        {
            return await _dbBug.AuditLogs
                            .OrderByDescending(log => log.ActionDate)
                            .Include(log => log.User)
                            .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetUserLogsAsync(int userId)
        {
            return await _dbBug.AuditLogs
                            .Where(log => log.UserId == userId)
                            .Include(log => log.User)
                            .OrderByDescending(log => log.ActionDate)
                            .ToListAsync();
        }
    }
}
