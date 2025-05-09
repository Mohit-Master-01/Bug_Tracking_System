using Bug_Tracking_System.Hubs;
using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories;
using Bug_Tracking_System.Repositories.AuthClasses;
using Bug_Tracking_System.Repositories.BugsClasses;
using Bug_Tracking_System.Repositories.Interfaces;
using Bug_Tracking_System.Repositories.MembersClasses;
using Bug_Tracking_System.Repositories.ProjectsClasses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddAuthorization();

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton<IEmailSenderRepos, EmailSenderClassRepos>(); // Email service interface and implementation




// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSignalR()
        .AddHubOptions<NotificationHub>(options =>
        {
            options.ClientTimeoutInterval = TimeSpan.FromMinutes(1); // optional tuning
        });

builder.Services.AddScoped<IAccountRepos, AccountClassRepos>();
builder.Services.AddScoped<ILoginRepos, LoginClassRepos>();
builder.Services.AddScoped<ISidebarRepos, SidebarClassRepos>();
builder.Services.AddScoped<IProjectsRepos, ProjectsClassRepos>();
builder.Services.AddScoped<IProfileRepos, ProfileClassRepos>();
builder.Services.AddScoped<IMembersRepos, MemberClassRepos>();
builder.Services.AddScoped<IBugRepos, BugsClassRepos>();
builder.Services.AddScoped<INotificationRepos, NotificationClassRepos>();
builder.Services.AddScoped<IAuditLogsRepos, AuditLogsClassRepos>();
builder.Services.AddScoped<IPermissionHelperRepos, PermissionHelperClassRepos>();
builder.Services.AddScoped<IImportRepos, ImportClassRepos>();
builder.Services.AddScoped<IExportRepos, ExportClassRepos>();
builder.Services.AddScoped<IDashboardRepos, DashboardClassRepos>();
builder.Services.AddScoped<GoogleCalendarService>();
builder.Services.AddScoped<ZoomService>();



AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);


builder.Services.AddDbContext<DbBug>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDistributedMemoryCache(); // Required for session storage

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true; // Ensures session cookie is accessible only via HTTP
    options.Cookie.IsEssential = true; // Ensures cookie is essential
});
builder.Services.AddHttpContextAccessor();


builder.Services.AddHttpClient();

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
})
.AddGoogle(options =>
{
    options.ClientId = "141974506519-j9agl9t56nbp7p47pgtgoba3f0g7q788.apps.googleusercontent.com";
    options.ClientSecret = "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";
    options.CallbackPath = "/signin-google"; // or whatever you’ve set

    options.Scope.Add("https://www.googleapis.com/auth/calendar");

    options.SaveTokens = true; // ?? REQUIRED for access_token   

    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub"); // ? This is critical
    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");

});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    //https://aka.ms/aspnetcore-hsts
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Enable Authentication & Authorization Middleware
app.UseAuthentication();

app.UseSession();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<NotificationHub>("/NotificationHub");

});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=LandingPage}/{id?}");

app.Run();
