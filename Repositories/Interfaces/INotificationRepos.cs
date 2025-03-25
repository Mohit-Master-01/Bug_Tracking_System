using Bug_Tracking_System.Models;

namespace Bug_Tracking_System.Repositories.Interfaces
{
    public interface INotificationRepos
    {
        Task<List<Notification>> GetNotificationByIdAsync(int userId); // Fetch unread notifications for a specific user
        Task AddNotification(int userId, int type, string message, int relatedId, string moduleType); // Add a new notification
        Task MarkAsReadAsync(int notificationId); // Mark a specific notification as read
        Task<int> GetUnreadNotificationCountAsync(int userId); // Get unread notification count for a user
    }
}
