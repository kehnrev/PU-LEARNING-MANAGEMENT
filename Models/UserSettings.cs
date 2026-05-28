using System.ComponentModel.DataAnnotations;

namespace EduTrackAnalytics.Models;

public class UserSettings
{
    public const string DefaultThemeMode = "Light";
    public const string DefaultLayoutStyle = "Comfortable";
    public const string DefaultSidebarState = "Expanded";
    public const string DefaultFontSize = "Medium";
    public const string DefaultCardStyle = "Default";
    public const string DefaultReminderTiming = "1 day before";
    public const string DefaultStudentDashboardPriority = "Upcoming Assessments";
    public const string DefaultTeacherDashboardPriority = "Students Needing Support";
    public const string DefaultAdminDashboardPriority = "System Statistics";
    public const string DefaultActiveSchoolYear = "2025-2026";
    public const string DefaultActiveSemester = "1st Semester";

    public static readonly string[] ThemeModes = ["Light", "Dark", "System"];
    public static readonly string[] LayoutStyles = ["Comfortable", "Compact"];
    public static readonly string[] SidebarStates = ["Expanded", "Collapsed", "Hidden"];
    public static readonly string[] FontSizes = ["Small", "Medium", "Large"];
    public static readonly string[] CardStyles = ["Default", "Minimal"];
    public static readonly string[] ReminderTimings = ["Same day", "1 day before", "2 days before", "1 week before"];
    public static readonly string[] StudentDashboardPriorities = ["Upcoming Assessments", "Recent Scores", "Course Progress", "Offline Lessons", "Announcements"];
    public static readonly string[] TeacherDashboardPriorities = ["Students Needing Support", "Pending Submissions", "Recent Submissions", "Upcoming Assessments", "Class Performance"];
    public static readonly string[] AdminDashboardPriorities = ["System Statistics", "Recent Users", "Reports Summary", "Announcements", "Overall Performance"];

    public int UserSettingsId { get; set; }

    public int UserId { get; set; }

    public ApplicationUser? User { get; set; }

    [Required, StringLength(20)]
    public string ThemeMode { get; set; } = DefaultThemeMode;

    [Required, StringLength(20)]
    public string LayoutStyle { get; set; } = DefaultLayoutStyle;

    [Required, StringLength(20)]
    public string SidebarState { get; set; } = DefaultSidebarState;

    [Required, StringLength(20)]
    public string FontSize { get; set; } = DefaultFontSize;

    [Required, StringLength(20)]
    public string CardStyle { get; set; } = DefaultCardStyle;

    public bool HighContrastMode { get; set; }

    public bool ReduceAnimations { get; set; }

    public bool ReadableSpacing { get; set; } = true;

    public bool EnableAssessmentReminders { get; set; } = true;

    public bool EnableNewLessonNotifications { get; set; } = true;

    public bool EnableScoreNotifications { get; set; } = true;

    public bool EnableAnnouncementNotifications { get; set; } = true;

    public bool EnableOfflineSyncNotifications { get; set; } = true;

    [Required, StringLength(30)]
    public string ReminderTiming { get; set; } = DefaultReminderTiming;

    public bool AutoSaveOfflineLessons { get; set; } = true;

    public bool ShowOfflineBanner { get; set; } = true;

    public bool AutoSyncWhenOnline { get; set; } = true;

    public bool ShowSyncSuccessMessage { get; set; } = true;

    public bool LowDataMode { get; set; }

    [Required, StringLength(60)]
    public string StudentDashboardPriority { get; set; } = DefaultStudentDashboardPriority;

    [Required, StringLength(60)]
    public string TeacherDashboardPriority { get; set; } = DefaultTeacherDashboardPriority;

    [Required, StringLength(60)]
    public string AdminDashboardPriority { get; set; } = DefaultAdminDashboardPriority;

    [Range(1, 100)]
    public int DefaultPassingScore { get; set; } = 75;

    [Range(1, 600)]
    public int DefaultAssessmentDuration { get; set; } = 30;

    public bool AllowLateSubmissions { get; set; }

    public bool ShowCorrectAnswersAfterSubmission { get; set; }

    public bool AutoPublishScores { get; set; } = true;

    public bool IncludePerformanceLabelsInReports { get; set; } = true;

    public bool IncludeMissingSubmissionsInReports { get; set; } = true;

    public bool IncludeTeacherRemarksInReports { get; set; } = true;

    public bool CompactPrintLayout { get; set; }

    public bool AlwaysPrintReportsInLightMode { get; set; } = true;

    public bool EnableStudentRegistration { get; set; } = true;

    public bool EnableOfflineMode { get; set; } = true;

    public bool EnableDemoDataNotice { get; set; } = true;

    [Required, StringLength(20)]
    public string ActiveSchoolYear { get; set; } = DefaultActiveSchoolYear;

    [Required, StringLength(30)]
    public string ActiveSemester { get; set; } = DefaultActiveSemester;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static UserSettings CreateDefault(int userId)
    {
        return new UserSettings
        {
            UserId = userId,
            ThemeMode = DefaultThemeMode,
            LayoutStyle = DefaultLayoutStyle,
            SidebarState = DefaultSidebarState,
            FontSize = DefaultFontSize,
            CardStyle = DefaultCardStyle,
            HighContrastMode = false,
            ReduceAnimations = false,
            ReadableSpacing = true,
            EnableAssessmentReminders = true,
            EnableNewLessonNotifications = true,
            EnableScoreNotifications = true,
            EnableAnnouncementNotifications = true,
            EnableOfflineSyncNotifications = true,
            ReminderTiming = DefaultReminderTiming,
            AutoSaveOfflineLessons = true,
            ShowOfflineBanner = true,
            AutoSyncWhenOnline = true,
            ShowSyncSuccessMessage = true,
            LowDataMode = false,
            StudentDashboardPriority = DefaultStudentDashboardPriority,
            TeacherDashboardPriority = DefaultTeacherDashboardPriority,
            AdminDashboardPriority = DefaultAdminDashboardPriority,
            DefaultPassingScore = 75,
            DefaultAssessmentDuration = 30,
            AllowLateSubmissions = false,
            ShowCorrectAnswersAfterSubmission = false,
            AutoPublishScores = true,
            IncludePerformanceLabelsInReports = true,
            IncludeMissingSubmissionsInReports = true,
            IncludeTeacherRemarksInReports = true,
            CompactPrintLayout = false,
            AlwaysPrintReportsInLightMode = true,
            EnableStudentRegistration = true,
            EnableOfflineMode = true,
            EnableDemoDataNotice = true,
            ActiveSchoolYear = DefaultActiveSchoolYear,
            ActiveSemester = DefaultActiveSemester,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static bool IsValidThemeMode(string? value) => ThemeModes.Contains(value);
    public static bool IsValidLayoutStyle(string? value) => LayoutStyles.Contains(value);
    public static bool IsValidSidebarState(string? value) => SidebarStates.Contains(value);
    public static bool IsValidFontSize(string? value) => FontSizes.Contains(value);
    public static bool IsValidCardStyle(string? value) => CardStyles.Contains(value);
    public static bool IsValidReminderTiming(string? value) => ReminderTimings.Contains(value);
    public static bool IsValidStudentDashboardPriority(string? value) => StudentDashboardPriorities.Contains(value);
    public static bool IsValidTeacherDashboardPriority(string? value) => TeacherDashboardPriorities.Contains(value);
    public static bool IsValidAdminDashboardPriority(string? value) => AdminDashboardPriorities.Contains(value);
}
