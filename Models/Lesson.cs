using System.ComponentModel.DataAnnotations;

namespace EduTrackAnalytics.Models;

public class Lesson
{
    public int LessonId { get; set; }

    [Display(Name = "Course")]
    public int CourseId { get; set; }

    public Course? Course { get; set; }

    [Required, StringLength(140)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(8000)]
    public string Content { get; set; } = string.Empty;

    [StringLength(400)]
    [Display(Name = "File path")]
    public string? FilePath { get; set; }

    [StringLength(255)]
    [Display(Name = "Original file name")]
    public string? OriginalFileName { get; set; }

    [StringLength(120)]
    public string? ContentType { get; set; }

    public long? FileSize { get; set; }

    [Display(Name = "Available offline")]
    public bool IsAvailableOffline { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}
