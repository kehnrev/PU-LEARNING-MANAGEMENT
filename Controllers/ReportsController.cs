using EduTrackAnalytics.Data;
using EduTrackAnalytics.Models;
using EduTrackAnalytics.Services;
using EduTrackAnalytics.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EduTrackAnalytics.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PerformanceAnalyticsService _analytics;

    public ReportsController(ApplicationDbContext context, PerformanceAnalyticsService analytics)
    {
        _context = context;
        _analytics = analytics;
    }

    public async Task<IActionResult> StudentPerformance(int? studentId)
    {
        var currentId = User.GetUserId();

        if (User.IsInRole("Student"))
        {
            studentId = currentId;
        }

        await LoadStudentSelectListAsync(studentId);

        var submissions = _context.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assessment)
            .ThenInclude(a => a!.Course)
            .AsQueryable();

        if (studentId.HasValue)
        {
            submissions = submissions.Where(s => s.StudentId == studentId.Value);
        }

        if (User.IsInRole("Teacher"))
        {
            var teacherId = currentId;
            submissions = submissions.Where(s => s.Assessment != null && s.Assessment.Course != null && s.Assessment.Course.TeacherId == teacherId);
        }

        var rows = await submissions.OrderByDescending(s => s.SubmittedAt).ToListAsync();
        var settings = await LoadReportSettingsAsync();
        ViewBag.Summary = BuildSummary("Student Performance Report", rows, settings.DefaultPassingScore);
        return View(rows);
    }

    public async Task<IActionResult> ClassPerformance(int? courseId)
    {
        await LoadCourseSelectListAsync(courseId);

        var submissions = _context.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assessment)
            .ThenInclude(a => a!.Course)
            .AsQueryable();

        if (courseId.HasValue)
        {
            submissions = submissions.Where(s => s.Assessment != null && s.Assessment.CourseId == courseId.Value);
        }

        if (User.IsInRole("Teacher"))
        {
            var teacherId = User.GetUserId();
            submissions = submissions.Where(s => s.Assessment != null && s.Assessment.Course != null && s.Assessment.Course.TeacherId == teacherId);
        }
        else if (User.IsInRole("Student"))
        {
            submissions = submissions.Where(s => s.StudentId == User.GetUserId());
        }

        var rows = await submissions.OrderBy(s => s.Assessment!.Course!.Title).ThenBy(s => s.Student!.FullName).ToListAsync();
        var settings = await LoadReportSettingsAsync();
        ViewBag.Summary = BuildSummary("Class Performance Report", rows, settings.DefaultPassingScore);
        return View(rows);
    }

    public async Task<IActionResult> CourseCompletion(int? courseId)
    {
        await LoadCourseSelectListAsync(courseId);

        var enrollments = _context.Enrollments
            .Include(e => e.Course)
            .ThenInclude(c => c!.Teacher)
            .Include(e => e.Student)
            .AsQueryable();

        if (courseId.HasValue)
        {
            enrollments = enrollments.Where(e => e.CourseId == courseId.Value);
        }

        if (User.IsInRole("Teacher"))
        {
            var teacherId = User.GetUserId();
            enrollments = enrollments.Where(e => e.Course != null && e.Course.TeacherId == teacherId);
        }
        else if (User.IsInRole("Student"))
        {
            enrollments = enrollments.Where(e => e.StudentId == User.GetUserId());
        }

        var rows = await enrollments.OrderBy(e => e.Course!.Title).ThenBy(e => e.Student!.FullName).ToListAsync();
        ViewBag.ReportSettings = await LoadReportSettingsAsync();
        ViewBag.GeneratedAt = DateTime.Now;
        ViewBag.AverageCompletion = rows.Count == 0 ? 0 : Math.Round(rows.Average(e => e.CompletionPercentage), 2);
        return View(rows);
    }

    public async Task<IActionResult> AssessmentResults(int? assessmentId)
    {
        await LoadAssessmentSelectListAsync(assessmentId);

        var submissions = _context.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assessment)
            .ThenInclude(a => a!.Course)
            .AsQueryable();

        if (assessmentId.HasValue)
        {
            submissions = submissions.Where(s => s.AssessmentId == assessmentId.Value);
        }

        if (User.IsInRole("Teacher"))
        {
            var teacherId = User.GetUserId();
            submissions = submissions.Where(s => s.Assessment != null && s.Assessment.Course != null && s.Assessment.Course.TeacherId == teacherId);
        }
        else if (User.IsInRole("Student"))
        {
            submissions = submissions.Where(s => s.StudentId == User.GetUserId());
        }

        var rows = await submissions.OrderByDescending(s => s.SubmittedAt).ToListAsync();
        var settings = await LoadReportSettingsAsync();
        ViewBag.Summary = BuildSummary("Assessment Result Report", rows, settings.DefaultPassingScore);
        return View(rows);
    }

    private static ReportSummaryViewModel BuildSummary(string title, IReadOnlyCollection<Submission> submissions, int passingScore)
    {
        return new ReportSummaryViewModel
        {
            Title = title,
            DateGenerated = DateTime.Now,
            TotalRows = submissions.Count,
            AverageScore = submissions.Count == 0 ? 0 : Math.Round(submissions.Average(s => s.Score), 2),
            PassedCount = submissions.Count(s => s.Score >= passingScore),
            FailedCount = submissions.Count(s => s.Score < passingScore)
        };
    }

    private async Task<UserSettings> LoadReportSettingsAsync()
    {
        var userId = User.GetUserId();
        var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            settings = UserSettings.CreateDefault(userId);
            _context.UserSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        ViewBag.ReportSettings = settings;
        return settings;
    }

    private async Task LoadStudentSelectListAsync(int? selectedId)
    {
        ViewBag.Students = new SelectList(
            await _context.Users.Where(u => u.Role == UserRole.Student).OrderBy(u => u.FullName).ToListAsync(),
            "Id",
            "FullName",
            selectedId);
    }

    private async Task LoadCourseSelectListAsync(int? selectedId)
    {
        var courses = _context.Courses.OrderBy(c => c.Title).AsQueryable();

        if (User.IsInRole("Teacher"))
        {
            var teacherId = User.GetUserId();
            courses = courses.Where(c => c.TeacherId == teacherId);
        }

        ViewBag.Courses = new SelectList(await courses.ToListAsync(), "CourseId", "Title", selectedId);
    }

    private async Task LoadAssessmentSelectListAsync(int? selectedId)
    {
        var assessments = _context.Assessments.Include(a => a.Course).OrderBy(a => a.Title).AsQueryable();

        if (User.IsInRole("Teacher"))
        {
            var teacherId = User.GetUserId();
            assessments = assessments.Where(a => a.Course != null && a.Course.TeacherId == teacherId);
        }

        ViewBag.Assessments = new SelectList(await assessments.ToListAsync(), "AssessmentId", "Title", selectedId);
    }
}
