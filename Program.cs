using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories;
using Bug_Tracking_System.Repositories.AuthClasses;
using Bug_Tracking_System.Repositories.Interfaces;
using Bug_Tracking_System.Repositories.ProjectsClasses;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddSingleton<IEmailSenderRepos, EmailSenderClassRepos>(); // Email service interface and implementation




// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IAccountRepos, AccountClassRepos>();
builder.Services.AddScoped<ILoginRepos, LoginClassRepos>();
builder.Services.AddScoped<ISidebarRepos, SidebarClassRepos>();
builder.Services.AddScoped<IProjectsRepos, ProjectsClassRepos>();

AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);


builder.Services.AddDbContext<DbBug>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.UseSession();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Registration}/{id?}");

app.Run();
