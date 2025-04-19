using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;


namespace Bug_Tracking_System.Repositories
{
    public class GoogleCalendarService
    {
        private readonly string[] Scopes = { CalendarService.Scope.Calendar };
        private const string ApplicationName = "Bug Tracking Calendar";
        private readonly IWebHostEnvironment _env;

        public GoogleCalendarService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task AddEventAsync(string summary, string description, DateTime start, DateTime end)
        {
            UserCredential credential;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
            }

            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var calendarEvent = new Event
            {
                Summary = summary,
                Description = description,
                Start = new EventDateTime() { DateTime = start, TimeZone = "Asia/Kolkata" },
                End = new EventDateTime() { DateTime = end, TimeZone = "Asia/Kolkata" }
            };

            await service.Events.Insert(calendarEvent, "primary").ExecuteAsync();
        }
    }
}
