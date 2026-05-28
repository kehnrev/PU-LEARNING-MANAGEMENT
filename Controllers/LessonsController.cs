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
    private readonly IWebHostEnvironment _environment;
    private static readonly string[] AllowedMaterialExtensions = [".pdf", ".doc", ".docx", ".ppt", ".pptx", ".txt"];
    private const long MaxMaterialFileSize = 10 * 1024 * 1024;

    public LessonsController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
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

    public async Task<IActionResult> Material(int id)
    {
        var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.LessonId == id);

        if (lesson == null || string.IsNullOrWhiteSpace(lesson.FilePath))
        {
            return NotFound();
        }

        if (!await CanViewCourseAsync(lesson.CourseId))
        {
            return Forbid();
        }

        if (!lesson.FilePath.StartsWith("/uploads/lessons/", StringComparison.OrdinalIgnoreCase))
        {
            return Redirect(lesson.FilePath);
        }

        var relativePath = lesson.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var physicalPath = Path.Combine(_environment.WebRootPath, relativePath);

        if (!System.IO.File.Exists(physicalPath))
        {
            return NotFound();
        }

        return PhysicalFile(
            physicalPath,
            lesson.ContentType ?? "application/octet-stream",
            lesson.OriginalFileName ?? Path.GetFileName(physicalPath));
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
    public async Task<IActionResult> Create(Lesson lesson, IFormFile? materialFile)
    {
        if (!await CanModifyCourseAsync(lesson.CourseId))
        {
            return Forbid();
        }

        if (await _context.Lessons.AnyAsync(l => l.CourseId == lesson.CourseId && l.Title == lesson.Title))
        {
            ModelState.AddModelError(nameof(lesson.Title), "This course already has a lesson with this title.");
        }

        ValidateMaterialFile(materialFile);

        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync(lesson.CourseId);
            return View(lesson);
        }

        lesson.DateCreated = DateTime.UtcNow;

        try
        {
            await SaveMaterialFileAsync(lesson, materialFile);
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Lecture / Lesson added successfully.";
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
    public async Task<IActionResult> Edit(int id, Lesson lesson, IFormFile? materialFile)
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

        ValidateMaterialFile(materialFile);

        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync(lesson.CourseId);
            lesson.FilePath = existing.FilePath;
            lesson.OriginalFileName = existing.OriginalFileName;
            lesson.ContentType = existing.ContentType;
            lesson.FileSize = existing.FileSize;
            return View(lesson);
        }

        var postedFilePath = lesson.FilePath;
        lesson.DateCreated = existing.DateCreated;
        lesson.FilePath = postedFilePath;
        lesson.OriginalFileName = postedFilePath == existing.FilePath ? existing.OriginalFileName : null;
        lesson.ContentType = postedFilePath == existing.FilePath ? existing.ContentType : null;
        lesson.FileSize = postedFilePath == existing.FilePath ? existing.FileSize : null;

        try
        {
            var previousFilePath = existing.FilePath;
            await SaveMaterialFileAsync(lesson, materialFile);
            _context.Update(lesson);
            await _context.SaveChangesAsync();
            if (materialFile != null || postedFilePath != existing.FilePath)
            {
                DeleteUploadedFile(previousFilePath, lesson.FilePath);
            }
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
            DeleteUploadedFile(lesson.FilePath, null);
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

    private void ValidateMaterialFile(IFormFile? materialFile)
    {
        if (materialFile == null || materialFile.Length == 0)
        {
            return;
        }

        var extension = Path.GetExtension(materialFile.FileName).ToLowerInvariant();

        if (!AllowedMaterialExtensions.Contains(extension))
        {
            ModelState.AddModelError("materialFile", "Only PDF, Word, PowerPoint, and text files are allowed.");
        }

        if (materialFile.Length > MaxMaterialFileSize)
        {
            ModelState.AddModelError("materialFile", "File size must not exceed 10 MB.");
        }
    }

    private async Task SaveMaterialFileAsync(Lesson lesson, IFormFile? materialFile)
    {
        if (materialFile == null || materialFile.Length == 0)
        {
            return;
        }

        var extension = Path.GetExtension(materialFile.FileName).ToLowerInvariant();
        var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "lessons");
        Directory.CreateDirectory(uploadsRoot);

        var safeFileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadsRoot, safeFileName);

        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await materialFile.CopyToAsync(stream);
        }

        lesson.FilePath = $"/uploads/lessons/{safeFileName}";
        lesson.OriginalFileName = Path.GetFileName(materialFile.FileName);
        lesson.ContentType = materialFile.ContentType;
        lesson.FileSize = materialFile.Length;
    }

    private void DeleteUploadedFile(string? oldFilePath, string? replacementFilePath)
    {
        if (string.IsNullOrWhiteSpace(oldFilePath) ||
            oldFilePath == replacementFilePath ||
            !oldFilePath.StartsWith("/uploads/lessons/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            var relativePath = oldFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.Combine(_environment.WebRootPath, relativePath);

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }
        catch
        {
            // File cleanup should never block lesson deletion or replacement.
        }
    }
}
