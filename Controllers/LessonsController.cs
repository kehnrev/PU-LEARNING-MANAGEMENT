using EduTrackAnalytics.Data;
using EduTrackAnalytics.Models;
using EduTrackAnalytics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EduTrackAnalytics.Controllers;

[Authorize]
public class LessonsController : Controller
{
    private readonly ApplicationDbContext _context;

    public LessonsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? courseId, string? search)
    {
        var lessons = _context.Lessons
            .Include(l => l.Course)
            .ThenInclude(c => c!.Teacher)
            .AsQueryable();

        if (courseId.HasValue)
        {
            lessons = lessons.Where(l => l.CourseId == courseId.Value);
        }

        if (User.IsInRole("Teacher"))
        {
            var teacherId = User.GetUserId();
            lessons = lessons.Where(l => l.Course != null && l.Course.TeacherId == teacherId);
        }
        else if (User.IsInRole("Student"))
        {
            var studentId = User.GetUserId();
            lessons = lessons.Where(l => l.Course != null && l.Course.Enrollments.Any(e => e.StudentId == studentId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            lessons = lessons.Where(l => l.Title.Contains(search) || l.Content.Contains(search));
        }

        ViewBag.CourseId = courseId;
        ViewBag.Search = search;
        return View(await lessons.OrderByDescending(l => l.DateCreated).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.LessonId == id);

        if (lesson == null)
        {
            return NotFound();
        }

        if (!await CanViewCourseAsync(lesson.CourseId))
        {
            return Forbid();
        }

        return View(lesson);
    }

    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Create(int? courseId)
    {
        await LoadCoursesAsync(courseId);
        return View(new Lesson { CourseId = courseId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Create(Lesson lesson)
    {
        if (!await CanModifyCourseAsync(lesson.CourseId))
        {
            return Forbid();
        }

        if (await _context.Lessons.AnyAsync(l => l.CourseId == lesson.CourseId && l.Title == lesson.Title))
        {
            ModelState.AddModelError(nameof(lesson.Title), "This course already has a lesson with this title.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync(lesson.CourseId);
            return View(lesson);
        }

        lesson.DateCreated = DateTime.UtcNow;

        try
        {
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Lecture added successfully.";
            return RedirectToAction(nameof(Index), new { lesson.CourseId });
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "The lesson could not be saved. Please try again.");
            await LoadCoursesAsync(lesson.CourseId);
            return View(lesson);
        }
    }

    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Edit(int id)
    {
        var lesson = await _context.Lessons.FindAsync(id);

        if (lesson == null)
        {
            return NotFound();
        }

        if (!await CanModifyCourseAsync(lesson.CourseId))
        {
            return Forbid();
        }

        await LoadCoursesAsync(lesson.CourseId);
        return View(lesson);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Edit(int id, Lesson lesson)
    {
        if (id != lesson.LessonId)
        {
            return BadRequest();
        }

        var existing = await _context.Lessons.AsNoTracking().FirstOrDefaultAsync(l => l.LessonId == id);

        if (existing == null)
        {
            return NotFound();
        }

        if (!await CanModifyCourseAsync(existing.CourseId) || !await CanModifyCourseAsync(lesson.CourseId))
        {
            return Forbid();
        }

        if (await _context.Lessons.AnyAsync(l => l.LessonId != id && l.CourseId == lesson.CourseId && l.Title == lesson.Title))
        {
            ModelState.AddModelError(nameof(lesson.Title), "This course already has a lesson with this title.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync(lesson.CourseId);
            return View(lesson);
        }

        lesson.DateCreated = existing.DateCreated;

        try
        {
            _context.Update(lesson);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Lecture / Lesson updated.";
            return RedirectToAction(nameof(Index), new { lesson.CourseId });
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "The lesson could not be updated. Please try again.");
            await LoadCoursesAsync(lesson.CourseId);
            return View(lesson);
        }
    }

    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Delete(int id)
    {
        var lesson = await _context.Lessons.Include(l => l.Course).FirstOrDefaultAsync(l => l.LessonId == id);

        if (lesson == null)
        {
            return NotFound();
        }

        if (!await CanModifyCourseAsync(lesson.CourseId))
        {
            return Forbid();
        }

        return View(lesson);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var lesson = await _context.Lessons.FindAsync(id);

        if (lesson == null)
        {
            return NotFound();
        }

        if (!await CanModifyCourseAsync(lesson.CourseId))
        {
            return Forbid();
        }

        try
        {
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Lecture / Lesson deleted.";
        }
        catch (Exception)
        {
            TempData["Error"] = "The lesson could not be deleted.";
        }

        return RedirectToAction(nameof(Index), new { lesson.CourseId });
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
}
