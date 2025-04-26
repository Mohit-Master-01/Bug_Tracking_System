using Bug_Tracking_System.Hubs;
using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bug_Tracking_System.Controllers
{
    
    public class NotificationController : BaseController
    {
        private readonly INotificationRepos _notification;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationController(IHubContext<NotificationHub> hubContext, INotificationRepos notification, ISidebarRepos sidebar)
            : base(sidebar)
        {
            _notification = notification;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> SendNotification(int userId, string message, int type, int relatedId, string moduleType)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(message))
                return BadRequest(new { success = false, error = "Invalid data" });

            try
            {
                // Add notification to database
                await _notification.AddNotification(userId, type, message, relatedId, moduleType);

                // Send real-time notification via SignalR
                await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", message);

                return Ok(new { success = true, message = "Notification sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An error occurred while sending notification", details = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            if (notificationId <= 0)
                return BadRequest(new { success = false, error = "Invalid Notification ID" });

            await _notification.MarkAsReadAsync(notificationId);
            return Ok(new { success = true });
        }

        [HttpGet, ActionName("Notifications")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
                {
                    return Unauthorized(new { error = "User not authenticated" });
                }

                var notifications = await _notification.GetNotificationByIdAsync(userId);
                return Json(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }




        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Json(new { count = 0 }); // return 0 if not authenticated
                }

                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
                {
                    return Json(new { count = 0 });
                }

                int count = await _notification.GetUnreadNotificationCountAsync(userId);
                return Json(new { count = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}
