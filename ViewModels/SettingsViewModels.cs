using System.ComponentModel.DataAnnotations;

namespace EduTrackAnalytics.ViewModels;

public class AppearanceSettingsViewModel
{
    [Required, StringLength(120)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Role")]
    public string Role { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Theme mode")]
    public string ThemeMode { get; set; } = "Light";

    [Required]
    [Display(Name = "Layout style")]
    public string LayoutStyle { get; set; } = "Comfortable";

    [Required]
    [Display(Name = "Sidebar preference")]
    public string SidebarState { get; set; } = "Expanded";

    [Required]
    [Display(Name = "Font size")]
    public string FontSize { get; set; } = "Medium";

    [Required]
    [Display(Name = "Dashboard card style")]
    public string CardStyle { get; set; } = "Default";

    [Display(Name = "High contrast mode")]
    public bool HighContrastMode { get; set; }

    [Display(Name = "Reduce animations")]
    public bool ReduceAnimations { get; set; }

    [Display(Name = "Readable spacing")]
    public bool ReadableSpacing { get; set; } = true;

    [Display(Name = "Assessment reminders")]
    public bool EnableAssessmentReminders { get; set; } = true;

    [Display(Name = "New lesson notifications")]
    public bool EnableNewLessonNotifications { get; set; } = true;

    [Display(Name = "Score release notifications")]
    public bool EnableScoreNotifications { get; set; } = true;

    [Display(Name = "Announcement notifications")]
    public bool EnableAnnouncementNotifications { get; set; } = true;

    [Display(Name = "Offline sync notifications")]
    public bool EnableOfflineSyncNotifications { get; set; } = true;

    [Required]
    [Display(Name = "Reminder timing")]
    public string ReminderTiming { get; set; } = "1 day before";

    [Display(Name = "Auto-save offline lessons")]
    public bool AutoSaveOfflineLessons { get; set; } = true;

    [Display(Name = "Show offline mode banner")]
    public bool ShowOfflineBanner { get; set; } = true;

    [Display(Name = "Sync pending submissions when online")]
    public bool AutoSyncWhenOnline { get; set; } = true;

    [Display(Name = "Show sync success message")]
    public bool ShowSyncSuccessMessage { get; set; } = true;

    [Display(Name = "Prefer low-data mode")]
    public bool LowDataMode { get; set; }

    [Required]
    [Display(Name = "Student dashboard priority")]
    public string StudentDashboardPriority { get; set; } = "Upcoming Assessments";

    [Required]
    [Display(Name = "Teacher dashboard priority")]
    public string TeacherDashboardPriority { get; set; } = "Students Needing Support";

    [Required]
    [Display(Name = "Admin dashboard priority")]
    public string AdminDashboardPriority { get; set; } = "System Statistics";

    [Range(1, 100)]
    [Display(Name = "Default passing score")]
    public int DefaultPassingScore { get; set; } = 75;

    [Range(1, 600)]
    [Display(Name = "Default assessment duration")]
    public int DefaultAssessmentDuration { get; set; } = 30;

    [Display(Name = "Allow late submissions")]
    public bool AllowLateSubmissions { get; set; }

    [Display(Name = "Show correct answers after submission")]
    public bool ShowCorrectAnswersAfterSubmission { get; set; }

    [Display(Name = "Auto-publish scores after submission")]
    public bool AutoPublishScores { get; set; } = true;

    [Display(Name = "Include performance labels")]
    public bool IncludePerformanceLabelsInReports { get; set; } = true;

    [Display(Name = "Include missing submissions")]
    public bool IncludeMissingSubmissionsInReports { get; set; } = true;

    [Display(Name = "Include teacher remarks")]
    public bool IncludeTeacherRemarksInReports { get; set; } = true;

    [Display(Name = "Use compact print layout")]
    public bool CompactPrintLayout { get; set; }

    [Display(Name = "Always print reports in light mode")]
    public bool AlwaysPrintReportsInLightMode { get; set; } = true;

    [Display(Name = "Enable student registration")]
    public bool EnableStudentRegistration { get; set; } = true;

    [Display(Name = "Enable offline mode")]
    public bool EnableOfflineMode { get; set; } = true;

    [Display(Name = "Enable demo data notice")]
    public bool EnableDemoDataNotice { get; set; } = true;

    [Required, StringLength(20)]
    [Display(Name = "Active school year")]
    public string ActiveSchoolYear { get; set; } = "2025-2026";

    [Required, StringLength(30)]
    [Display(Name = "Active semester")]
    public string ActiveSemester { get; set; } = "1st Semester";

    public DateTime? UpdatedAt { get; set; }
}

public class QuickThemeViewModel
{
    public string ThemeMode { get; set; } = "Light";
}

public class QuickSidebarViewModel
{
    public string SidebarState { get; set; } = "Expanded";
}
