using Bug_Tracking_System.DTOs;

namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface IDashboardRepos
    {
        Task<DashboardDTO> GetDashboardData(int userId, int role);
    }
}
