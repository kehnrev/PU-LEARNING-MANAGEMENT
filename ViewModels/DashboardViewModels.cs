using EduTrackAnalytics.Models;

namespace EduTrackAnalytics.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalCourses { get; set; }
    public int TotalAssessments { get; set; }
    public decimal OverallAveragePerformance { get; set; }
    public int PendingSyncSubmissions { get; set; }
    public List<Announcement> RecentAnnouncements { get; set; } = new();
    public Dictionary<string, int> CourseEnrollmentSummary { get; set; } = new();
    public Dictionary<string, int> PassFailDistribution { get; set; } = new();
}

public class TeacherDashboardViewModel
{
    public int MyCourses { get; set; }
    public int TotalStudentsEnrolled { get; set; }
    public int TotalLessonsCreated { get; set; }
    public int TotalAssessmentsCreated { get; set; }
    public int PendingSubmissions { get; set; }
    public decimal AverageClassScore { get; set; }
    public List<ApplicationUser> StudentsNeedingImprovement { get; set; } = new();
    public List<Assessment> RecentAssessments { get; set; } = new();
    public List<Submission> RecentSubmissions { get; set; } = new();
    public List<Assessment> UpcomingAssessments { get; set; } = new();
    public Dictionary<string, decimal> ClassAverageByCourse { get; set; } = new();
    public Dictionary<string, int> SubmissionStatusSummary { get; set; } = new();
}

public class TeacherClassListViewModel
{
    public int? CourseId { get; set; }
    public string? Search { get; set; }
    public List<Course> Courses { get; set; } = new();
    public List<TeacherStudentCourseRowViewModel> Students { get; set; } = new();
}

public class TeacherStudentProgressViewModel
{
    public int? CourseId { get; set; }
    public string? Search { get; set; }
    public int TotalStudents { get; set; }
    public decimal ClassAverage { get; set; }
    public int MissingSubmissions { get; set; }
    public int StudentsNeedingSupport { get; set; }
    public List<Course> Courses { get; set; } = new();
    public List<TeacherStudentCourseRowViewModel> StudentPerformance { get; set; } = new();
    public List<TeacherStudentSupportViewModel> SupportRows { get; set; } = new();
    public List<Submission> RecentSubmissions { get; set; } = new();
    public Dictionary<string, decimal> CourseProgress { get; set; } = new();
    public Dictionary<string, int> MissingSubmissionsByCourse { get; set; } = new();
}

public class TeacherStudentCourseRowViewModel
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public decimal AverageScore { get; set; }
    public decimal ProgressPercentage { get; set; }
    public int MissingAssessments { get; set; }
}

public class TeacherStudentSupportViewModel
{
    public string StudentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public decimal AverageScore { get; set; }
    public decimal ProgressPercentage { get; set; }
    public int MissingAssessments { get; set; }
    public string SuggestedAction { get; set; } = string.Empty;
}

public class StudentDashboardViewModel
{
    public int EnrolledCourses { get; set; }
    public int UpcomingAssessments { get; set; }
    public decimal AverageScore { get; set; }
    public decimal ProgressPercentage { get; set; }
    public int OfflineAvailableLessons { get; set; }
    public int PendingSyncSubmissions { get; set; }
    public List<Course> Courses { get; set; } = new();
    public List<Assessment> Assessments { get; set; } = new();
    public List<Submission> RecentScores { get; set; } = new();
    public Dictionary<string, decimal> CourseProgress { get; set; } = new();
    public Dictionary<string, decimal> RecentAssessmentScores { get; set; } = new();
    public int CompletedAssessments { get; set; }
    public int RemainingAssessments { get; set; }
}

public class OfflineLibraryViewModel
{
    public List<Lesson> Lessons { get; set; } = new();
    public List<Assessment> Assessments { get; set; } = new();
}
