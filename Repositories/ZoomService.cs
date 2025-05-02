using System;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using ZoomNet;
using ZoomNet.Models;
using ZoomNet.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Bug_Tracking_System.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using Bug_Tracking_System.Repositories.Interfaces;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http; // add this namespace



namespace Bug_Tracking_System.Repositories
{
    public class ZoomService
    {

        private readonly DbBug _dbBug;
        private readonly SmtpSettings _smtpSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotificationRepos _notification;
        //private readonly IAuditLogsRepos _auditLogs;

        private readonly string _accountId = "kuMeAgwfQp-NbfmTcwyPQw";
        private readonly string _clientId = "iv4vgzdgTU6BLtKymROohg";
        private readonly string _clientSecret = "0WZ6uNqYaTgT3QUf3pYZD3NChZNgnDqm";

        public ZoomService(DbBug dbBug, INotificationRepos notificationRepos, IOptions<SmtpSettings> smtpSettings, IHttpContextAccessor httpContextAccessor)
        {
            _dbBug = dbBug;
            _smtpSettings = smtpSettings.Value;
            _httpContextAccessor = httpContextAccessor;
            _notification = notificationRepos;
            // _auditLogs = auditLogs;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            using var client = new HttpClient();

            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "account_credentials"),
                new KeyValuePair<string, string>("account_id", _accountId)
            });

            var response = await client.PostAsync("https://zoom.us/oauth/token", content);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tokenResult = JsonSerializer.Deserialize<JsonElement>(json);

            return tokenResult.GetProperty("access_token").GetString();
        }

        public async Task<string> CreateMeeting(string topic, DateTime startTime)
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Instead of GET /users/me separately, directly use "me" as userId
                string userId = "me";

                var meetingData = new
                {
                    topic = topic,
                    type = 2, // Scheduled meeting
                    start_time = startTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    duration = 30, // in minutes
                    timezone = "Asia/Kolkata",
                    settings = new
                    {
                        join_before_host = true
                    }
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(meetingData), Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"https://api.zoom.us/v2/users/{userId}/meetings", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Zoom API Error: " + errorContent);
                    throw new Exception($"Zoom API Error: {errorContent}");
                }


                var createdMeetingJson = await response.Content.ReadAsStringAsync();
                var createdMeeting = JsonSerializer.Deserialize<JsonElement>(createdMeetingJson);

                string joinUrl = createdMeeting.GetProperty("join_url").GetString();
                string meetingTopic = createdMeeting.GetProperty("topic").GetString();

                // 📢 After successful meeting creation -> Send emails to all active users
                await SendMeetingInvitationEmailsAsync(meetingTopic, startTime, joinUrl);

                // 📢 Now create Google Calendar event
                await CreateGoogleCalendarEventForMeeting(topic, startTime, joinUrl);

                // ✅ Send Notification to all active users
                var activeUsers = await _dbBug.Users
                    .Where(u => (bool)u.IsActive && !string.IsNullOrEmpty(u.Email)) // or your own logic
                    .ToListAsync();

                foreach (var user in activeUsers)
                {
                    await _notification.AddNotification(
                        user.UserId,
                        type: 2, // You can define a custom type for meetings
                        message: $"📢 A Zoom meeting '{meetingTopic}' has been scheduled at {startTime:hh:mm tt}.",
                        relatedId: 0, // Optional: You can store Meeting ID if you want
                        moduleType: "Meeting"
                    );
                }

                return joinUrl;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error while creating the meeting: " + ex.Message, ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Error while parsing the response from Zoom API: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred: " + ex.Message, ex);
            }
        }

        private async Task SendMeetingInvitationEmailsAsync(string topic, DateTime startTime, string joinUrl)
        {            
            var members = await _dbBug.Users.Where(u => u.IsActive == true && u.RoleId != 4).ToListAsync();

            using (var smtpClient = new SmtpClient())
            {
                await smtpClient.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_smtpSettings.SenderEmail, _smtpSettings.SenderPassword);

                foreach (var member in members)
                {
                    if (string.IsNullOrWhiteSpace(member.Email) || !member.Email.Contains("@"))
                        continue; // Skip invalid emails

                    try
                    {
                        string subject = $"Invitation: {topic} - Scheduled Meeting";

                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress("Bugify - The Bug Tracking System", _smtpSettings.SenderEmail));
                        message.To.Add(new MailboxAddress("User", member.Email));
                        message.Subject = subject;

                        // Correctly use the body
                        string body = $@"
                                    <div style='max-width:600px;margin:auto;font-family:Segoe UI,Tahoma,Geneva,Verdana,sans-serif;background-color:#f9fbfd;
                                                border:1px solid #dce3ec;border-radius:10px;padding:25px;color:#333;'>
                                        <div style='text-align:center;padding-bottom:20px;'>
                                            <img src='https://img.icons8.com/color/96/zoom.png' alt='Zoom Icon' style='width:60px;margin-bottom:10px;' />
                                            <h2 style='margin:0;color:#2c3e50;'>Zoom Meeting Invitation</h2>
                                        </div>
                                        <div style='line-height:1.6;'>
                                            <p>Dear <strong>{member.UserName}</strong>,</p>
                                            <p>You are invited to join an upcoming <strong>Zoom Meeting</strong>:</p>

                                            <div style='background-color:#ecf3fe;padding:15px 20px;margin:20px 0;
                                                        border-left:4px solid #3b82f6;border-radius:5px;'>
                                                <p style='margin:8px 0;'><strong>📌 Topic:</strong> {topic}</p>
                                                <p style='margin:8px 0;'><strong>📅 Date & Time:</strong> {startTime:dd-MM-yyyy hh:mm tt}</p>
                                                <p style='margin:8px 0;'><strong>🕒 Timezone:</strong> Asia/Kolkata</p>
                                                <p style='margin:8px 0;'><strong>🔗 Join Link:</strong> 
                                                    <a href='{joinUrl}' target='_blank' style='color:#0c63e4;font-weight:bold;text-decoration:none;'>
                                                        Click here to join
                                                    </a>
                                                </p>
                                            </div>

                                            <p>Please be ready to join on time. Click the join link above to access the meeting.</p>
                                            <p>Need assistance? Feel free to reply to this email.</p>
                                            <p>We look forward to your presence!</p>
                                        </div>
                                        <div style='text-align:center;font-size:0.9em;color:#777;margin-top:30px;
                                                    border-top:1px solid #ccc;padding-top:15px;'>
                                            &copy; {DateTime.Now.Year} Bugify. All rights reserved.
                                        </div>
                                    </div>";


                        var bodyBuilder = new BodyBuilder
                        {
                            HtmlBody = body
                        };
                        message.Body = bodyBuilder.ToMessageBody();

                        await smtpClient.SendAsync(message);
                    }
                    catch (SmtpCommandException ex) when (ex.Message.Contains("Daily user sending limit exceeded"))
                    {
                        Console.WriteLine("Daily limit exceeded. Stopping email sending for today.");
                        break; // STOP loop immediately
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send email to {member.Email}: {ex.Message}");
                        // Optionally: Log the error to your database or file
                    }
                }

                await smtpClient.DisconnectAsync(true);
            }
        }


        private async Task CreateGoogleCalendarEventForMeeting(string topic, DateTime startTime, string joinUrl)
        {
            try
            {
                // Get the AccessToken for Google from Session
                var accessToken = _httpContextAccessor.HttpContext.Session.GetString("GoogleAccessToken");

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("Google access token not available. Please login via Google.");
                }

                // Setup Google credential
                var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromAccessToken(accessToken);

                var calendarService = new Google.Apis.Calendar.v3.CalendarService(new Google.Apis.Services.BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Bug Tracking Calendar"
                });

                // Create the Calendar Event
                var newEvent = new Google.Apis.Calendar.v3.Data.Event()
                {
                    Summary = topic,
                    Description = $"Zoom Meeting Link: {joinUrl}",
                    Start = new Google.Apis.Calendar.v3.Data.EventDateTime()
                    {
                        DateTime = startTime,
                        TimeZone = "Asia/Kolkata"
                    },
                    End = new Google.Apis.Calendar.v3.Data.EventDateTime()
                    {
                        DateTime = startTime.AddMinutes(30),
                        TimeZone = "Asia/Kolkata"
                    },
                    Reminders = new Google.Apis.Calendar.v3.Data.Event.RemindersData()
                    {
                        UseDefault = false,
                        Overrides = new List<Google.Apis.Calendar.v3.Data.EventReminder>()
                {
                    new Google.Apis.Calendar.v3.Data.EventReminder() { Method = "popup", Minutes = 10 },
                    new Google.Apis.Calendar.v3.Data.EventReminder() { Method = "email", Minutes = 15 }
                }
                    }
                };

                var insertRequest = calendarService.Events.Insert(newEvent, "primary");
                await insertRequest.ExecuteAsync();
            }
            catch (Google.GoogleApiException gex)
            {
                Console.WriteLine("Google API Error while creating Calendar event: " + gex.Message);
                // You can also log this to a logging system
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating Google Calendar event: " + ex.Message);
                throw;
            }
        }



    }
}
