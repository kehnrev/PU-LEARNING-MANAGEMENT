using EduTrackAnalytics.Data;
using EduTrackAnalytics.Models;
using EduTrackAnalytics.Services;
using EduTrackAnalytics.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrackAnalytics.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Appearance()
    {
        var user = await GetCurrentUserAsync();

        if (user == null)
        {
            return Forbid();
        }

        var settings = await GetOrCreateSettingsAsync(user.Id);
        return View(ToViewModel(settings, user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Appearance(AppearanceSettingsViewModel model)
    {
        ValidateSettings(model);
        var user = await GetCurrentUserAsync();

        if (user == null)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            model.Email = user.Email;
            model.Role = user.Role.ToString();
            ModelState.AddModelError(string.Empty, "Invalid setting value. Please check your selection.");
            return View(model);
        }

        var settings = await GetOrCreateSettingsAsync(user.Id);
        user.FullName = model.FullName.Trim();
        settings.ThemeMode = model.ThemeMode;
        settings.LayoutStyle = model.LayoutStyle;
        settings.SidebarState = model.SidebarState;
        settings.FontSize = model.FontSize;
        settings.CardStyle = model.CardStyle;
        settings.HighContrastMode = model.HighContrastMode;
        settings.ReduceAnimations = model.ReduceAnimations;
        settings.ReadableSpacing = model.ReadableSpacing;
        settings.EnableAssessmentReminders = model.EnableAssessmentReminders;
        settings.EnableNewLessonNotifications = model.EnableNewLessonNotifications;
        settings.EnableScoreNotifications = model.EnableScoreNotifications;
        settings.EnableAnnouncementNotifications = model.EnableAnnouncementNotifications;
        settings.EnableOfflineSyncNotifications = model.EnableOfflineSyncNotifications;
        settings.ReminderTiming = model.ReminderTiming;
        settings.AutoSaveOfflineLessons = model.AutoSaveOfflineLessons;
        settings.ShowOfflineBanner = model.ShowOfflineBanner;
        settings.AutoSyncWhenOnline = model.AutoSyncWhenOnline;
        settings.ShowSyncSuccessMessage = model.ShowSyncSuccessMessage;
        settings.LowDataMode = model.LowDataMode;
        settings.StudentDashboardPriority = model.StudentDashboardPriority;
        settings.TeacherDashboardPriority = model.TeacherDashboardPriority;
        settings.AdminDashboardPriority = model.AdminDashboardPriority;
        settings.DefaultPassingScore = model.DefaultPassingScore;
        settings.DefaultAssessmentDuration = model.DefaultAssessmentDuration;
        settings.AllowLateSubmissions = model.AllowLateSubmissions;
        settings.ShowCorrectAnswersAfterSubmission = model.ShowCorrectAnswersAfterSubmission;
        settings.AutoPublishScores = model.AutoPublishScores;
        settings.IncludePerformanceLabelsInReports = model.IncludePerformanceLabelsInReports;
        settings.IncludeMissingSubmissionsInReports = model.IncludeMissingSubmissionsInReports;
        settings.IncludeTeacherRemarksInReports = model.IncludeTeacherRemarksInReports;
        settings.CompactPrintLayout = model.CompactPrintLayout;
        settings.AlwaysPrintReportsInLightMode = model.AlwaysPrintReportsInLightMode;

        if (user.Role == UserRole.Admin)
        {
            settings.EnableStudentRegistration = model.EnableStudentRegistration;
            settings.EnableOfflineMode = model.EnableOfflineMode;
            settings.EnableDemoDataNotice = model.EnableDemoDataNotice;
            settings.ActiveSchoolYear = model.ActiveSchoolYear.Trim();
            settings.ActiveSemester = model.ActiveSemester.Trim();
        }

        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Settings saved successfully.";
        return RedirectToAction(nameof(Appearance));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickTheme([FromBody] QuickThemeViewModel model)
    {
        if (!UserSettings.IsValidThemeMode(model.ThemeMode))
        {
            return BadRequest(new { success = false, message = "Choose Light Mode, Dark Mode, or System Default." });
        }

        var settings = await GetOrCreateSettingsAsync(User.GetUserId());
        settings.ThemeMode = model.ThemeMode;
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Settings saved successfully." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickSidebar([FromBody] QuickSidebarViewModel model)
    {
        if (!UserSettings.IsValidSidebarState(model.SidebarState))
        {
            return BadRequest(new { success = false, message = "Choose Expanded Sidebar, Collapsed Sidebar, or Hidden Sidebar." });
        }

        var settings = await GetOrCreateSettingsAsync(User.GetUserId());
        settings.SidebarState = model.SidebarState;
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Settings saved successfully." });
    }


    private async Task<UserSettings> GetOrCreateSettingsAsync(int userId)
    {
        var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings != null)
        {
            return settings;
        }

        settings = UserSettings.CreateDefault(userId);
        _context.UserSettings.Add(settings);
        await _context.SaveChangesAsync();
        return settings;
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == User.GetUserId() && u.IsActive);
    }

    private static AppearanceSettingsViewModel ToViewModel(UserSettings settings, ApplicationUser user)
    {
        return new AppearanceSettingsViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            ThemeMode = settings.ThemeMode,
            LayoutStyle = settings.LayoutStyle,
            SidebarState = settings.SidebarState,
            FontSize = settings.FontSize,
            CardStyle = settings.CardStyle,
            HighContrastMode = settings.HighContrastMode,
            ReduceAnimations = settings.ReduceAnimations,
            ReadableSpacing = settings.ReadableSpacing,
            EnableAssessmentReminders = settings.EnableAssessmentReminders,
            EnableNewLessonNotifications = settings.EnableNewLessonNotifications,
            EnableScoreNotifications = settings.EnableScoreNotifications,
            EnableAnnouncementNotifications = settings.EnableAnnouncementNotifications,
            EnableOfflineSyncNotifications = settings.EnableOfflineSyncNotifications,
            ReminderTiming = settings.ReminderTiming,
            AutoSaveOfflineLessons = settings.AutoSaveOfflineLessons,
            ShowOfflineBanner = settings.ShowOfflineBanner,
            AutoSyncWhenOnline = settings.AutoSyncWhenOnline,
            ShowSyncSuccessMessage = settings.ShowSyncSuccessMessage,
            LowDataMode = settings.LowDataMode,
            StudentDashboardPriority = settings.StudentDashboardPriority,
            TeacherDashboardPriority = settings.TeacherDashboardPriority,
            AdminDashboardPriority = settings.AdminDashboardPriority,
            DefaultPassingScore = settings.DefaultPassingScore,
            DefaultAssessmentDuration = settings.DefaultAssessmentDuration,
            AllowLateSubmissions = settings.AllowLateSubmissions,
            ShowCorrectAnswersAfterSubmission = settings.ShowCorrectAnswersAfterSubmission,
            AutoPublishScores = settings.AutoPublishScores,
            IncludePerformanceLabelsInReports = settings.IncludePerformanceLabelsInReports,
            IncludeMissingSubmissionsInReports = settings.IncludeMissingSubmissionsInReports,
            IncludeTeacherRemarksInReports = settings.IncludeTeacherRemarksInReports,
            CompactPrintLayout = settings.CompactPrintLayout,
            AlwaysPrintReportsInLightMode = settings.AlwaysPrintReportsInLightMode,
            EnableStudentRegistration = settings.EnableStudentRegistration,
            EnableOfflineMode = settings.EnableOfflineMode,
            EnableDemoDataNotice = settings.EnableDemoDataNotice,
            ActiveSchoolYear = settings.ActiveSchoolYear,
            ActiveSemester = settings.ActiveSemester,
            UpdatedAt = settings.UpdatedAt
        };
    }

    private void ValidateSettings(AppearanceSettingsViewModel model)
    {
        if (!UserSettings.IsValidThemeMode(model.ThemeMode))
        {
            ModelState.AddModelError(nameof(model.ThemeMode), "Choose Light Mode, Dark Mode, or System Default.");
        }

        if (!UserSettings.IsValidLayoutStyle(model.LayoutStyle))
        {
            ModelState.AddModelError(nameof(model.LayoutStyle), "Choose Comfortable Layout or Compact Layout.");
        }

        if (!UserSettings.IsValidSidebarState(model.SidebarState))
        {
            ModelState.AddModelError(nameof(model.SidebarState), "Choose Expanded Sidebar, Collapsed Sidebar, or Hidden Sidebar.");
        }

        if (!UserSettings.IsValidFontSize(model.FontSize))
        {
            ModelState.AddModelError(nameof(model.FontSize), "Choose Small, Medium, or Large font size.");
        }

        if (!UserSettings.IsValidCardStyle(model.CardStyle))
        {
            ModelState.AddModelError(nameof(model.CardStyle), "Choose Default cards or Minimal cards.");
        }

        if (!UserSettings.IsValidReminderTiming(model.ReminderTiming))
        {
            ModelState.AddModelError(nameof(model.ReminderTiming), "Choose Same day, 1 day before, 2 days before, or 1 week before.");
        }

        if (!UserSettings.IsValidStudentDashboardPriority(model.StudentDashboardPriority))
        {
            ModelState.AddModelError(nameof(model.StudentDashboardPriority), "Choose a valid student dashboard priority.");
        }

        if (!UserSettings.IsValidTeacherDashboardPriority(model.TeacherDashboardPriority))
        {
            ModelState.AddModelError(nameof(model.TeacherDashboardPriority), "Choose a valid teacher dashboard priority.");
        }

        if (!UserSettings.IsValidAdminDashboardPriority(model.AdminDashboardPriority))
        {
            ModelState.AddModelError(nameof(model.AdminDashboardPriority), "Choose a valid admin dashboard priority.");
        }

        if (model.DefaultPassingScore is < 1 or > 100)
        {
            ModelState.AddModelError(nameof(model.DefaultPassingScore), "Passing score must be between 1 and 100.");
        }

        if (model.DefaultAssessmentDuration <= 0)
        {
            ModelState.AddModelError(nameof(model.DefaultAssessmentDuration), "Assessment duration must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(model.ActiveSchoolYear))
        {
            ModelState.AddModelError(nameof(model.ActiveSchoolYear), "Active school year cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(model.ActiveSemester))
        {
            ModelState.AddModelError(nameof(model.ActiveSemester), "Active semester cannot be empty.");
        }
    }
}
