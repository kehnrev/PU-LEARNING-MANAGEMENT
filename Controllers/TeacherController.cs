using EduTrackAnalytics.Data;
using EduTrackAnalytics.Models;
using EduTrackAnalytics.Services;
using EduTrackAnalytics.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrackAnalytics.Controllers;

[Authorize(Roles = "Teacher")]
public class TeacherController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PerformanceAnalyticsService _analytics;

    public TeacherController(ApplicationDbContext context, PerformanceAnalyticsService analytics)
    {
        _context = context;
        _analytics = analytics;
    }

    public async Task<IActionResult> Index()
    {
        var teacherId = User.GetUserId();
        var courses = await _context.Courses
            .Where(c => c.TeacherId == teacherId)
            .OrderBy(c => c.Title)
            .ToListAsync();
        var courseIds = courses.Select(c => c.CourseId).ToList();
        var classAverageByCourse = new Dictionary<string, decimal>();

        foreach (var course in courses)
        {
            classAverageByCourse[course.Title] = await _analytics.GetCourseAverageAsync(course.CourseId);
        }

        var expectedSubmissions = 0;

        foreach (var course in courses)
        {
            var enrolled = await _context.Enrollments.CountAsync(e => e.CourseId == course.CourseId);
            var assessments = await _context.Assessments.CountAsync(a => a.CourseId == course.CourseId);
            expectedSubmissions += enrolled * assessments;
        }

        var actualSubmissions = await _context.Submissions
            .CountAsync(s => s.Assessment != null && courseIds.Contains(s.Assessment.CourseId));

        var model = new TeacherDashboardViewModel
        {
            MyCourses = courseIds.Count,
            TotalStudentsEnrolled = await _context.Enrollments
                .Where(e => courseIds.Contains(e.CourseId))
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync(),
            TotalLessonsCreated = await _context.Lessons.CountAsync(l => courseIds.Contains(l.CourseId)),
            TotalAssessmentsCreated = await _context.Assessments.CountAsync(a => courseIds.Contains(a.CourseId)),
            PendingSubmissions = Math.Max(0, expectedSubmissions - actualSubmissions),
            AverageClassScore = await _analytics.GetTeacherAverageAsync(teacherId),
            StudentsNeedingImprovement = await _analytics.GetStudentsNeedingImprovementAsync(teacherId),
            RecentAssessments = await _context.Assessments
                .Include(a => a.Course)
                .Where(a => courseIds.Contains(a.CourseId))
                .OrderByDescending(a => a.DueDate)
                .Take(5)
                .ToListAsync(),
            RecentSubmissions = await _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Assessment)
                .ThenInclude(a => a!.Course)
                .Where(s => s.Assessment != null && courseIds.Contains(s.Assessment.CourseId))
                .OrderByDescending(s => s.SubmittedAt)
                .Take(6)
                .ToListAsync(),
            UpcomingAssessments = await _context.Assessments
                .Include(a => a.Course)
                .Where(a => courseIds.Contains(a.CourseId) && a.DueDate >= DateTime.UtcNow)
                .OrderBy(a => a.DueDate)
                .Take(6)
                .ToListAsync(),
            ClassAverageByCourse = classAverageByCourse,
            SubmissionStatusSummary = new Dictionary<string, int>
            {
                ["Submitted"] = actualSubmissions,
                ["Missing"] = Math.Max(0, expectedSubmissions - actualSubmissions)
            }
        };

        return View(model);
    }

    public async Task<IActionResult> ClassList(int? courseId, string? search)
    {
        var teacherId = User.GetUserId();
        var courses = await GetTeacherCoursesAsync(teacherId);

        if (courseId.HasValue && courses.All(c => c.CourseId != courseId.Value))
        {
            return Forbid();
        }

        var model = new TeacherClassListViewModel
        {
            CourseId = courseId,
            Search = search,
            Courses = courses,
            Students = await BuildClassRowsAsync(teacherId, courseId, search)
        };

        return View(model);
    }

    public async Task<IActionResult> StudentProgress(int? courseId, string? search)
    {
        var teacherId = User.GetUserId();
        var courses = await GetTeacherCoursesAsync(teacherId);

        if (courseId.HasValue && courses.All(c => c.CourseId != courseId.Value))
        {
            return Forbid();
        }

        var rows = await BuildClassRowsAsync(teacherId, courseId, search);
        var supportRows = rows
            .Where(r => r.AverageScore < 60 || r.ProgressPercentage < 60 || r.MissingAssessments > 0)
            .Select(r => new TeacherStudentSupportViewModel
            {
                StudentName = r.StudentName,
                Email = r.Email,
                CourseTitle = r.CourseTitle,
                AverageScore = r.AverageScore,
                ProgressPercentage = r.ProgressPercentage,
                MissingAssessments = r.MissingAssessments,
                SuggestedAction = GetSuggestedAction(r)
            })
            .OrderBy(r => r.AverageScore)
            .ThenByDescending(r => r.MissingAssessments)
            .ToList();

        var courseIds = courses.Select(c => c.CourseId).ToList();
        var recentSubmissions = await _context.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assessment)
            .ThenInclude(a => a!.Course)
            .Where(s => s.Assessment != null && courseIds.Contains(s.Assessment.CourseId))
            .OrderByDescending(s => s.SubmittedAt)
            .Take(10)
            .ToListAsync();

        var model = new TeacherStudentProgressViewModel
        {
            CourseId = courseId,
            Search = search,
            Courses = courses,
            StudentPerformance = rows,
            SupportRows = supportRows,
            RecentSubmissions = recentSubmissions,
            TotalStudents = rows.Select(r => r.StudentId).Distinct().Count(),
            ClassAverage = rows.Count == 0 ? 0 : Math.Round(rows.Average(r => r.AverageScore), 2),
            MissingSubmissions = rows.Sum(r => r.MissingAssessments),
            StudentsNeedingSupport = supportRows.Select(r => r.Email).Distinct().Count(),
            CourseProgress = rows
                .GroupBy(r => r.CourseTitle)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => Math.Round(g.Average(r => r.ProgressPercentage), 2)),
            MissingSubmissionsByCourse = rows
                .GroupBy(r => r.CourseTitle)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.MissingAssessments))
        };

        return View(model);
    }

    private async Task<List<Course>> GetTeacherCoursesAsync(int teacherId)
    {
        return await _context.Courses
            .Where(c => c.TeacherId == teacherId)
            .OrderBy(c => c.Title)
            .ToListAsync();
    }

    private async Task<List<TeacherStudentCourseRowViewModel>> BuildClassRowsAsync(
        int teacherId,
        int? courseId,
        string? search)
    {
        var enrollments = _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Where(e => e.Course != null && e.Course.TeacherId == teacherId);

        if (courseId.HasValue)
        {
            enrollments = enrollments.Where(e => e.CourseId == courseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            enrollments = enrollments.Where(e =>
                e.Student != null &&
                (e.Student.FullName.Contains(term) || e.Student.Email.Contains(term)));
        }

        var enrollmentList = await enrollments
            .OrderBy(e => e.Course!.Title)
            .ThenBy(e => e.Student!.FullName)
            .ToListAsync();

        var rows = new List<TeacherStudentCourseRowViewModel>();

        foreach (var enrollment in enrollmentList)
        {
            var totalAssessments = await _context.Assessments.CountAsync(a => a.CourseId == enrollment.CourseId);
            var scoreQuery = _context.Submissions
                .Where(s => s.StudentId == enrollment.StudentId && s.Assessment != null && s.Assessment.CourseId == enrollment.CourseId);
            var submittedAssessments = await scoreQuery
                .Select(s => s.AssessmentId)
                .Distinct()
                .CountAsync();
            var averageScore = await scoreQuery.Select(s => (decimal?)s.Score).AverageAsync() ?? 0;
            var assessmentProgress = totalAssessments == 0
                ? enrollment.CompletionPercentage
                : Math.Round((decimal)submittedAssessments / totalAssessments * 100, 2);
            var progress = totalAssessments == 0
                ? enrollment.CompletionPercentage
                : Math.Round((assessmentProgress + enrollment.CompletionPercentage) / 2, 2);

            rows.Add(new TeacherStudentCourseRowViewModel
            {
                StudentId = enrollment.StudentId,
                CourseId = enrollment.CourseId,
                StudentName = enrollment.Student?.FullName ?? "Student",
                Email = enrollment.Student?.Email ?? string.Empty,
                CourseTitle = enrollment.Course?.Title ?? "Course",
                GradeLevel = enrollment.Course?.GradeLevel ?? string.Empty,
                AverageScore = Math.Round(averageScore, 2),
                ProgressPercentage = progress,
                MissingAssessments = Math.Max(0, totalAssessments - submittedAssessments)
            });
        }

        return rows;
    }

    private static string GetSuggestedAction(TeacherStudentCourseRowViewModel row)
    {
        if (row.MissingAssessments > 0)
        {
            return "Remind student to submit pending assessment";
        }

        if (row.ProgressPercentage < 60)
        {
            return "Review recent lessons";
        }

        if (row.AverageScore < 60)
        {
            return "Provide additional learning support";
        }

        return "Continue monitoring progress";
    }
}
