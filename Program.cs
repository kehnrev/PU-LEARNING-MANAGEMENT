using EduTrackAnalytics.Data;
using EduTrackAnalytics.Models;
using EduTrackAnalytics.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var sqlServerConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
var fallbackEnabled = builder.Environment.IsDevelopment()
    && (builder.Configuration.GetValue("Database:UseFallbackWhenSqlServerUnavailable", false)
        || IsCodexWorkspace(builder.Environment.ContentRootPath));
string? sqlServerStartupError = null;
var useInMemoryFallback = fallbackEnabled && !CanOpenSqlServer(sqlServerConnectionString, out sqlServerStartupError);

if (useInMemoryFallback)
{
    Console.Error.WriteLine(
        $"SQL Server / LocalDB is unavailable. Using EF Core InMemory fallback for this development run only. Details: {sqlServerStartupError}");
}

builder.Services.AddControllersWithViews();
var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (useInMemoryFallback)
    {
        options.UseInMemoryDatabase("EduTrackAnalyticsDevelopmentFallback");
    }
    else
    {
        options.UseSqlServer(sqlServerConnectionString);
    }
});
builder.Services.AddScoped<PerformanceAnalyticsService>();
builder.Services.AddSingleton<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
builder.Services.AddSingleton(new DatabaseStartupState(useInMemoryFallback ? "InMemory" : "SqlServer"));

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/HttpStatus", "?code={0}");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    try
    {
        await SeedData.InitializeAsync(
            scope.ServiceProvider,
            app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", false),
            useInMemoryFallback);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Database seed skipped. The site will continue running. Details: {ex}");
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static bool CanOpenSqlServer(string connectionString, out string? error)
{
    try
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        error = null;
        return true;
    }
    catch (Exception ex)
    {
        error = ex.Message;
        return false;
    }
}

static bool IsCodexWorkspace(string contentRootPath)
{
    return contentRootPath.Contains($"{Path.DirectorySeparatorChar}Documents{Path.DirectorySeparatorChar}Codex{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || contentRootPath.Contains($"{Path.AltDirectorySeparatorChar}Documents{Path.AltDirectorySeparatorChar}Codex{Path.AltDirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}

public sealed record DatabaseStartupState(string Provider);
