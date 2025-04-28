using Microsoft.AspNetCore.Mvc;
using Bug_Tracking_System.Repositories;
using System.Threading.Tasks;


namespace Bug_Tracking_System.Controllers
{
    [Route("[controller]/[action]")]
    public class ZoomController : Controller
    {
        private readonly ZoomService _zoomService;

        public ZoomController(ZoomService zoomService)
        {
            _zoomService = zoomService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Topic) || request.StartTime == default)
                {
                    return BadRequest("Invalid data");
                }

                var meetingUrl = await _zoomService.CreateMeeting(request.Topic, request.StartTime);

                return Json(new { meetingUrl });
            }
            catch (Exception ex)
            {
                // Log the exception and return a 500 internal server error response
                // Optionally, you can provide more detailed error messages
                return StatusCode(500, new { message = "An error occurred while creating the meeting: " + ex.Message });
            }
        }
    }

    public class CreateMeetingRequest
    {
        public string Topic { get; set; }
        public DateTime StartTime { get; set; }
    }
}
