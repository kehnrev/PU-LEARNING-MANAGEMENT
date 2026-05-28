using System.Security.Claims;
using EduTrackAnalytics.Data;
using EduTrackAnalytics.Models;
using EduTrackAnalytics.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrackAnalytics.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        ApplicationDbContext context,
        IPasswordHasher<ApplicationUser> passwordHasher,
        ILogger<AccountController> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

        if (user == null ||
            _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        try
        {
            await SignInAsync(user, model.RememberMe);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed while signing in user {Email}.", user.Email);
            ModelState.AddModelError(string.Empty, "We could not sign you in right now. Please try again.");
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToRoleDashboard(user.Role);
    }

    public async Task<IActionResult> Register()
    {
        if (!await IsStudentRegistrationEnabledAsync())
        {
            TempData["Error"] = "Student registration is currently disabled by the administrator.";
            return RedirectToAction(nameof(Login));
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!await IsStudentRegistrationEnabledAsync())
        {
            ModelState.AddModelError(string.Empty, "Student registration is currently disabled by the administrator.");
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await _context.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            FullName = model.FullName,
            Email = model.Email,
            Role = UserRole.Student,
            IsActive = true,
            DateCreated = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await SignInAsync(user, isPersistent: false);
            TempData["Success"] = "Your student account is ready. Welcome to EduTrack.";
            return RedirectToAction("Index", "Student");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for email {Email}.", model.Email);
            ModelState.AddModelError(string.Empty, "We could not create your account right now. Please try again.");
            return View(model);
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        return await SignOutAndRedirectAsync();
    }

    [Authorize]
    [HttpPost]
    [ActionName("Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutPost()
    {
        return await SignOutAndRedirectAsync();
    }

    private async Task<IActionResult> SignOutAndRedirectAsync()
    {
        try
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed while clearing the authentication cookie.");
        }

        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task SignInAsync(ApplicationUser user, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });
    }

    private IActionResult RedirectToRoleDashboard(UserRole role)
    {
        return role switch
        {
            UserRole.Admin => RedirectToAction("Index", "Admin"),
            UserRole.Teacher => RedirectToAction("Index", "Teacher"),
            UserRole.Student => RedirectToAction("Index", "Student"),
            _ => RedirectToAction(nameof(AccessDenied))
        };
    }

    private async Task<bool> IsStudentRegistrationEnabledAsync()
    {
        var adminIds = await _context.Users
            .Where(u => u.Role == UserRole.Admin && u.IsActive)
            .Select(u => u.Id)
            .ToListAsync();

        if (adminIds.Count == 0)
        {
            return true;
        }

        return !await _context.UserSettings
            .AnyAsync(s => adminIds.Contains(s.UserId) && !s.EnableStudentRegistration);
    }
}
