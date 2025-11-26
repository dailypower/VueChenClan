using BulkyBook.DataAccess.DbInitializer;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.DataAcess.Data;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Data.Sqlite; // Add this line
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.Extensions.DependencyInjection; // Add this line
using Stripe;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("Data Source="))
{
    //(3) SqliteDB  2025.03.26 11:49
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.ConfigureWarnings(warnings =>

        warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored));
    });
}
else
{
    //(2)MSSQLLocalDB or Azure_MSSQLServerDB 2025.03.26 11:49
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));
}

//if (connectionString.Contains("Data Source="))
//{
//    //(3) SqliteDB  2025.03.26 11:49
//  builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlite(builder.Configuration["ConnectionStrings:DefaultConnection"]));
//}
//else
//{
//    //(2)MSSQLLocalDB or Azure_MSSQLServerDB 2025.03.26 11:49
//    builder.Services.AddDbContext<ApplicationDbContext>(options =>
//        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//}

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});
builder.Services.AddAuthentication().AddFacebook(option =>
{
    option.AppId = "193813826680436";
    option.AppSecret = "8fc42ae3f4f2a4986143461d4e2da919";
});
builder.Services.AddAuthentication().AddMicrosoftAccount(option =>
{
    option.ClientId = "ec4d380d-d631-465d-b473-1e26ee706331";
    option.ClientSecret = "YOUR_MICROSOFT_CLIENT_SECRET_HERE";
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
SeedDatabase();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}"
    );
app.Run();
void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}

/// <summary>
/// Public partial class to allow tests access to Program configuration.
/// </summary>
public partial class Program { }

