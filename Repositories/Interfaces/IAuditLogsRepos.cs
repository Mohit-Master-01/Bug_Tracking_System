using System.Collections.Generic;
using System.Threading.Tasks;
using Bug_Tracking_System.Models;

namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface IAuditLogsRepos
    {
        Task AddAuditLogAsync(int userId, string action, string module);
        Task<IEnumerable<AuditLog>> GetUserLogsAsync(int userId);
        Task<IEnumerable<AuditLog>> GetAllLogsAsync();
    }
}
