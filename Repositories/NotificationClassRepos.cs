using Bug_Tracking_System.Hubs;
using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bug_Tracking_System.Repositories
{
    public class NotificationClassRepos : INotificationRepos
    {
        private readonly DbBug _dbBug;
        private readonly IHubContext<NotificationHub> _hubContext;


        public NotificationClassRepos(DbBug dbBug, IHubContext<NotificationHub> hubContext) 
        {
            _dbBug = dbBug;
            _hubContext = hubContext;
        }

        public async Task AddNotification(int userId, int type, string message, int relatedId, string moduleType)
        {
                var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Message = message,
                RelatedId = relatedId,
                ModuleType = moduleType,
                IsRead = false,
                NotificationDate = DateTime.Now
            };

            _dbBug.Notifications.Add(notification);
            await _dbBug.SaveChangesAsync();

            // Send real-time notification
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", message);
        }

        public async Task<List<Notification>> GetNotificationByIdAsync(int userId)
        {
            return await _dbBug.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.NotificationDate)
                .ToListAsync();
        }

        public async Task<int> GetUnreadNotificationCountAsync(int userId)
        {
            return await _dbBug.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _dbBug.Notifications.FirstOrDefaultAsync(n => n.NotificationId == notificationId);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _dbBug.SaveChangesAsync();
            }
        }
    }
}
