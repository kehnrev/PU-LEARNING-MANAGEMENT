using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrackAnalytics.Migrations
{
    /// <inheritdoc />
    public partial class ExpandUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActiveSchoolYear",
                table: "UserSettings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "2025-2026");

            migrationBuilder.AddColumn<string>(
                name: "ActiveSemester",
                table: "UserSettings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "1st Semester");

            migrationBuilder.AddColumn<string>(
                name: "AdminDashboardPriority",
                table: "UserSettings",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "System Statistics");

            migrationBuilder.AddColumn<bool>(
                name: "AllowLateSubmissions",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AlwaysPrintReportsInLightMode",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoPublishScores",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoSaveOfflineLessons",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoSyncWhenOnline",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "CompactPrintLayout",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DefaultAssessmentDuration",
                table: "UserSettings",
                type: "int",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<int>(
                name: "DefaultPassingScore",
                table: "UserSettings",
                type: "int",
                nullable: false,
                defaultValue: 75);

            migrationBuilder.AddColumn<bool>(
                name: "EnableAnnouncementNotifications",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableAssessmentReminders",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableDemoDataNotice",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableNewLessonNotifications",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableOfflineMode",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableOfflineSyncNotifications",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableScoreNotifications",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableStudentRegistration",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "HighContrastMode",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeMissingSubmissionsInReports",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncludePerformanceLabelsInReports",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeTeacherRemarksInReports",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "LowDataMode",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReadableSpacing",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReduceAnimations",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReminderTiming",
                table: "UserSettings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "1 day before");

            migrationBuilder.AddColumn<bool>(
                name: "ShowCorrectAnswersAfterSubmission",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowOfflineBanner",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowSyncSuccessMessage",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentDashboardPriority",
                table: "UserSettings",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "Upcoming Assessments");

            migrationBuilder.AddColumn<string>(
                name: "TeacherDashboardPriority",
                table: "UserSettings",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "Students Needing Support");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveSchoolYear",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ActiveSemester",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "AdminDashboardPriority",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "AllowLateSubmissions",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "AlwaysPrintReportsInLightMode",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "AutoPublishScores",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "AutoSaveOfflineLessons",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "AutoSyncWhenOnline",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "CompactPrintLayout",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "DefaultAssessmentDuration",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "DefaultPassingScore",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "EnableAnnouncementNotifications",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "EnableAssessmentReminders",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "EnableDemoDataNotice",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "EnableNewLessonNotifications",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "EnableOfflineMode",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "EnableOfflineSyncNotifications",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "EnableScoreNotifications",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "EnableStudentRegistration",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "HighContrastMode",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "IncludeMissingSubmissionsInReports",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "IncludePerformanceLabelsInReports",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "IncludeTeacherRemarksInReports",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "LowDataMode",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ReadableSpacing",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ReduceAnimations",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ReminderTiming",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ShowCorrectAnswersAfterSubmission",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ShowOfflineBanner",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ShowSyncSuccessMessage",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "StudentDashboardPriority",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "TeacherDashboardPriority",
                table: "UserSettings");
        }
    }
}
