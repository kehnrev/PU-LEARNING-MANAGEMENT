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
public class AssessmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AssessmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? courseId, string? search)
    {
        var assessments = _context.Assessments
            .Include(a => a.Course)
            .ThenInclude(c => c!.Teacher)
            .Include(a => a.Questions)
            .Include(a => a.Submissions)
            .AsQueryable();

        if (courseId.HasValue)
        {
            assessments = assessments.Where(a => a.CourseId == courseId.Value);
        }

        if (User.IsInRole("Teacher"))
        {
            var teacherId = User.GetUserId();
            assessments = assessments.Where(a => a.Course != null && a.Course.TeacherId == teacherId);
        }
        else if (User.IsInRole("Student"))
        {
            var studentId = User.GetUserId();
            assessments = assessments.Where(a => a.Course != null && a.Course.Enrollments.Any(e => e.StudentId == studentId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            assessments = assessments.Where(a => a.Title.Contains(search) || a.Instructions.Contains(search));
        }

        ViewBag.CourseId = courseId;
        ViewBag.Search = search;
        return View(await assessments.OrderBy(a => a.DueDate).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var assessment = await _context.Assessments
            .Include(a => a.Course)
            .ThenInclude(c => c!.Teacher)
            .Include(a => a.Questions)
            .Include(a => a.Submissions)
            .ThenInclude(s => s.Student)
            .FirstOrDefaultAsync(a => a.AssessmentId == id);

        if (assessment == null)
        {
            return NotFound();
        }

        if (!await CanViewCourseAsync(assessment.CourseId))
        {
            return Forbid();
        }

        return View(assessment);
    }

    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Create(int? courseId)
    {
        await LoadCoursesAsync(courseId);
        var assessment = new Assessment { CourseId = courseId ?? 0, DueDate = DateTime.UtcNow.AddDays(7) };

        if (User.IsInRole("Teacher"))
        {
            var settings = await GetCurrentUserSettingsAsync();
            assessment.Instructions = $"Answer all questions. Time limit: {settings.DefaultAssessmentDuration} minutes. Passing score: {settings.DefaultPassingScore}%.";
            ViewBag.DefaultPassingScore = settings.DefaultPassingScore;
            ViewBag.DefaultAssessmentDuration = settings.DefaultAssessmentDuration;
            ViewBag.AllowLateSubmissions = settings.AllowLateSubmissions;
            ViewBag.AutoPublishScores = settings.AutoPublishScores;
        }

        return View(assessment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Create(Assessment assessment)
    {
        if (!await CanModifyCourseAsync(assessment.CourseId))
        {
            return Forbid();
        }

        ValidateAssessment(assessment);

        if (await _context.Assessments.AnyAsync(a => a.CourseId == assessment.CourseId && a.Title == assessment.Title))
        {
            ModelState.AddModelError(nameof(assessment.Title), "This course already has an assessment with this title.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync(assessment.CourseId);
            return View(assessment);
        }

        try
        {
            _context.Assessments.Add(assessment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Assessment created. Add questions next.";
            return RedirectToAction("Create", "Questions", new { assessmentId = assessment.AssessmentId });
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "The assessment could not be saved. Please try again.");
            await LoadCoursesAsync(assessment.CourseId);
            return View(assessment);
        }
    }

    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Edit(int id)
    {
        var assessment = await _context.Assessments.FindAsync(id);

        if (assessment == null)
        {
            return NotFound();
        }

        if (!await CanModifyCourseAsync(assessment.CourseId))
        {
            return Forbid();
        }

        await LoadCoursesAsync(assessment.CourseId);
        return View(assessment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Edit(int id, Assessment assessment)
    {
        if (id != assessment.AssessmentId)
        {
            return BadRequest();
        }

        var existing = await _context.Assessments.AsNoTracking().FirstOrDefaultAsync(a => a.AssessmentId == id);

        if (existing == null)
        {
            return NotFound();
        }

        if (!await CanModifyCourseAsync(existing.CourseId) || !await CanModifyCourseAsync(assessment.CourseId))
        {
            return Forbid();
        }

        ValidateAssessment(assessment);

        if (await _context.Assessments.AnyAsync(a => a.AssessmentId != id && a.CourseId == assessment.CourseId && a.Title == assessment.Title))
        {
            ModelState.AddModelError(nameof(assessment.Title), "This course already has an assessment with this title.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync(assessment.CourseId);
            return View(assessment);
        }

        try
        {
            _context.Update(assessment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Assessment updated.";
            return RedirectToAction(nameof(Details), new { id = assessment.AssessmentId });
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "The assessment could not be updated. Please try again.");
            await LoadCoursesAsync(assessment.CourseId);
            return View(assessment);
        }
    }

    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Delete(int id)
    {
        var assessment = await _context.Assessments.Include(a => a.Course).FirstOrDefaultAsync(a => a.AssessmentId == id);

        if (assessment == null)
        {
            return NotFound();
        }

        if (!await CanModifyCourseAsync(assessment.CourseId))
        {
            return Forbid();
        }

        return View(assessment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var assessment = await _context.Assessments.FindAsync(id);

        if (assessment == null)
        {
            return NotFound();
        }

        if (!await CanModifyCourseAsync(assessment.CourseId))
        {
            return Forbid();
        }

        try
        {
            _context.Assessments.Remove(assessment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Assessment deleted.";
        }
        catch (Exception)
        {
            TempData["Error"] = "The assessment could not be deleted because it has submissions.";
        }

        return RedirectToAction(nameof(Index), new { assessment.CourseId });
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Take(int id)
    {
        var model = await BuildTakeViewModelAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        if (!await IsStudentEnrolledAsync(model.AssessmentId, User.GetUserId()))
        {
            return Forbid();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Take(AssessmentTakeViewModel model)
    {
        if (!await IsStudentEnrolledAsync(model.AssessmentId, User.GetUserId()))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            var rebuilt = await BuildTakeViewModelAsync(model.AssessmentId);

            if (rebuilt == null)
            {
                return NotFound();
            }

            for (var i = 0; i < rebuilt.Questions.Count && i < model.Questions.Count; i++)
            {
                rebuilt.Questions[i].SelectedAnswer = model.Questions[i].SelectedAnswer;
            }

            return View(rebuilt);
        }

        var result = await SaveSubmissionAsync(model.AssessmentId, model.Questions.Select(q => new OfflineAnswerDto
        {
            QuestionId = q.QuestionId,
            SelectedAnswer = q.SelectedAnswer ?? string.Empty
        }).ToList(), User.GetUserId());

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = model.AssessmentId });
        }

        TempData["Success"] = $"Assessment submitted. Your score is {result.Score:0.##}%.";
        return RedirectToAction("Details", "Submissions", new { id = result.SubmissionId });
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> SyncOffline([FromBody] OfflineSubmissionDto submission)
    {
        if (submission == null || submission.AssessmentId == 0 || submission.Answers.Count == 0)
        {
            return BadRequest(new { success = false, message = "No answers were received." });
        }

        var studentId = User.GetUserId();

        if (!await IsStudentEnrolledAsync(submission.AssessmentId, studentId))
        {
            return Forbid();
        }

        var result = await SaveSubmissionAsync(submission.AssessmentId, submission.Answers, studentId);

        return Json(new
        {
            success = result.Success,
            message = result.Message,
            score = result.Score,
            submissionId = result.SubmissionId
        });
    }

    private async Task<AssessmentTakeViewModel?> BuildTakeViewModelAsync(int assessmentId)
    {
        var assessment = await _context.Assessments
            .Include(a => a.Questions.OrderBy(q => q.QuestionId))
            .FirstOrDefaultAsync(a => a.AssessmentId == assessmentId);

        if (assessment == null)
        {
            return null;
        }

        return new AssessmentTakeViewModel
        {
            AssessmentId = assessment.AssessmentId,
            Title = assessment.Title,
            Instructions = assessment.Instructions,
            DueDate = assessment.DueDate,
            TotalPoints = assessment.TotalPoints,
            IsAvailableOffline = assessment.IsAvailableOffline,
            Questions = assessment.Questions.Select(q => new TakeQuestionViewModel
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD
            }).ToList()
        };
    }

    private async Task<(bool Success, string Message, decimal Score, int SubmissionId)> SaveSubmissionAsync(
        int assessmentId,
        IReadOnlyCollection<OfflineAnswerDto> answers,
        int studentId)
    {
        var existing = await _context.Submissions
            .FirstOrDefaultAsync(s => s.AssessmentId == assessmentId && s.StudentId == studentId);

        if (existing != null)
        {
            return (true, "This assessment was already synced.", existing.Score, existing.SubmissionId);
        }

        var assessment = await _context.Assessments
            .Include(a => a.Questions)
            .FirstOrDefaultAsync(a => a.AssessmentId == assessmentId);

        if (assessment == null)
        {
            return (false, "Assessment was not found.", 0, 0);
        }

        var answerMap = answers
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => (g.First().SelectedAnswer ?? string.Empty).Trim().ToUpperInvariant());

        decimal earnedPoints = 0;
        var totalPoints = assessment.Questions.Sum(q => q.Points);
        var studentAnswers = new List<StudentAnswer>();

        foreach (var question in assessment.Questions)
        {
            answerMap.TryGetValue(question.QuestionId, out var selected);
            var isCorrect = selected == question.CorrectAnswer;
            var points = isCorrect ? question.Points : 0;

            earnedPoints += points;
            studentAnswers.Add(new StudentAnswer
            {
                QuestionId = question.QuestionId,
                SelectedAnswer = string.IsNullOrWhiteSpace(selected) ? "A" : selected,
                IsCorrect = isCorrect,
                PointsEarned = points
            });
        }

        var percentageScore = totalPoints == 0 ? 0 : Math.Round(earnedPoints / totalPoints * 100, 2);
        var submission = new Submission
        {
            AssessmentId = assessment.AssessmentId,
            StudentId = studentId,
            Score = percentageScore,
            SubmittedAt = DateTime.UtcNow,
            Status = SubmissionStatus.Synced,
            StudentAnswers = studentAnswers
        };

        try
        {
            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();
            return (true, "Submission synced successfully.", percentageScore, submission.SubmissionId);
        }
        catch (Exception)
        {
            return (false, "The submission could not be saved. Please sync again.", 0, 0);
        }
    }

    private void ValidateAssessment(Assessment assessment)
    {
        if (assessment.DueDate.Date < DateTime.UtcNow.Date)
        {
            ModelState.AddModelError(nameof(assessment.DueDate), "Due date cannot be in the past.");
        }
    }

    private async Task<bool> CanModifyCourseAsync(int courseId)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        var teacherId = User.GetUserId();
        return await _context.Courses.AnyAsync(c => c.CourseId == courseId && c.TeacherId == teacherId);
    }

    private async Task<bool> CanViewCourseAsync(int courseId)
    {
        if (await CanModifyCourseAsync(courseId))
        {
            return true;
        }

        var studentId = User.GetUserId();
        return await _context.Enrollments.AnyAsync(e => e.CourseId == courseId && e.StudentId == studentId);
    }

    private async Task<bool> IsStudentEnrolledAsync(int assessmentId, int studentId)
    {
        return await _context.Assessments
            .AnyAsync(a => a.AssessmentId == assessmentId && a.Course != null && a.Course.Enrollments.Any(e => e.StudentId == studentId));
    }

    private async Task LoadCoursesAsync(int? selectedCourseId = null)
    {
        var courses = _context.Courses.OrderBy(c => c.Title).AsQueryable();

        if (User.IsInRole("Teacher"))
        {
            var teacherId = User.GetUserId();
            courses = courses.Where(c => c.TeacherId == teacherId);
        }

        ViewBag.Courses = new SelectList(await courses.ToListAsync(), "CourseId", "Title", selectedCourseId);
    }

    private async Task<UserSettings> GetCurrentUserSettingsAsync()
    {
        var userId = User.GetUserId();
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
}
