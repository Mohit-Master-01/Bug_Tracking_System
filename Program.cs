using Bug_Tracking_System.Hubs;
using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories;
using Bug_Tracking_System.Repositories.AuthClasses;
using Bug_Tracking_System.Repositories.BugsClasses;
using Bug_Tracking_System.Repositories.Interfaces;
using Bug_Tracking_System.Repositories.MembersClasses;
using Bug_Tracking_System.Repositories.ProjectsClasses;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

var builder = WebApplication.CreateBuilder(args);

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Set your login page
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization();

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton<IEmailSenderRepos, EmailSenderClassRepos>(); // Email service interface and implementation




// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
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
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
