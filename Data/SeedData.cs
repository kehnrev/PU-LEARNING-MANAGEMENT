using EduTrackAnalytics.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduTrackAnalytics.Data;

public static class SeedData
{
    private const string AdminEmail = "admin@edutrack.local";
    private const string TeacherEmail = "teacher@edutrack.local";
    private const string StudentEmail = "student@edutrack.local";

    public static async Task InitializeAsync(
        IServiceProvider services,
        bool applyMigrationsOnStartup = false,
        bool ensureCreatedOnStartup = false)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher<ApplicationUser>>();

        try
        {
            if (ensureCreatedOnStartup)
            {
                await context.Database.EnsureCreatedAsync();
            }
            else if (applyMigrationsOnStartup)
            {
                await context.Database.MigrateAsync();
            }
            else if (!await context.Database.CanConnectAsync())
            {
                Console.Error.WriteLine("Database seed skipped because the configured database cannot be reached.");
                return;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Database initialization skipped. Details: {ex.Message}");
            return;
        }

        try
        {
            var admin = await EnsureUserAsync(context, passwordHasher, "System Administrator", AdminEmail, UserRole.Admin, "Admin123!");
            var teacher = await EnsureUserAsync(context, passwordHasher, "Maria Santos", TeacherEmail, UserRole.Teacher, "Teacher123!");
            var demoStudent = await EnsureUserAsync(context, passwordHasher, "Alex Reyes", StudentEmail, UserRole.Student, "Student123!");

            await EnsureUserAsync(context, passwordHasher, "Jamie Cruz", "student2@edutrack.local", UserRole.Student, "Student123!");
            await EnsureUserAsync(context, passwordHasher, "Taylor Lim", "student3@edutrack.local", UserRole.Student, "Student123!");
            await EnsureUserAsync(context, passwordHasher, "Rina Bautista", "student4@edutrack.local", UserRole.Student, "Student123!");
            await EnsureUserAsync(context, passwordHasher, "Jastin Percunod", "jastin.percunod@edutrack.local", UserRole.Student, "Student123!");
            await EnsureUserAsync(context, passwordHasher, "Kurt Recto", "kurt.recto@edutrack.local", UserRole.Student, "Student123!");
            await context.SaveChangesAsync();

            await RemoveLegacyDemoDataAsync(context, teacher.Id);

            var students = await context.Users
                .Where(u => u.Role == UserRole.Student && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();
            var users = await context.Users.Where(u => u.IsActive).ToListAsync();

            foreach (var user in users)
            {
                await EnsureUserSettingsAsync(context, user.Id);
            }

            students = students
                .OrderBy(u => string.Equals(u.Email, StudentEmail, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(u => u.FullName)
                .ToList();

            if (students.All(s => s.Id != demoStudent.Id))
            {
                students.Insert(0, demoStudent);
            }

            var courseSeeds = GetCourseSeeds();

            for (var courseIndex = 0; courseIndex < courseSeeds.Count; courseIndex++)
            {
                var seed = courseSeeds[courseIndex];
                var course = await EnsureCourseAsync(context, teacher.Id, seed, courseIndex);

                foreach (var lesson in BuildLessons(seed, courseIndex))
                {
                    await EnsureLessonAsync(context, course.CourseId, lesson, courseIndex);
                }

                foreach (var student in students)
                {
                    await EnsureEnrollmentAsync(
                        context,
                        course.CourseId,
                        student.Id,
                        GetCompletionPercentage(student, courseIndex));
                }

                var assessments = BuildAssessments(seed, courseIndex);

                for (var assessmentIndex = 0; assessmentIndex < assessments.Count; assessmentIndex++)
                {
                    var assessmentSeed = assessments[assessmentIndex];
                    var assessment = await EnsureAssessmentAsync(context, course.CourseId, assessmentSeed);

                    foreach (var question in BuildQuestions(seed, assessmentSeed))
                    {
                        await EnsureQuestionAsync(context, assessment.AssessmentId, question);
                    }

                    if (!assessmentSeed.CreateDemoSubmissions)
                    {
                        continue;
                    }

                    foreach (var student in students)
                    {
                        await EnsureSubmissionAsync(
                            context,
                            assessment.AssessmentId,
                            student.Id,
                            GetCorrectAnswerCount(student, courseIndex, assessmentIndex),
                            DateTime.UtcNow.AddDays(-(courseIndex + assessmentIndex + 2)),
                            GetSubmissionStatus(student, seed));
                    }
                }
            }

            await EnsureAnnouncementAsync(
                context,
                "Welcome to EduTrack Analytics",
                "Welcome to the Senior High School demo workspace. Grade 11 and Grade 12 learners already have courses, lessons, quizzes, scores, and progress data for presentation testing.",
                admin.Id,
                DateTime.UtcNow.AddDays(-5));

            await EnsureAnnouncementAsync(
                context,
                "Upcoming Grade 11 and Grade 12 assessments",
                "Students can review the upcoming application quizzes in each enrolled course. Teachers can monitor submissions and class performance from the dashboard.",
                teacher.Id,
                DateTime.UtcNow.AddDays(-3));

            await EnsureAnnouncementAsync(
                context,
                "Offline learning materials are available",
                "Selected lessons and quizzes are marked Available Offline so students can continue learning during low-connectivity periods and sync work when online.",
                teacher.Id,
                DateTime.UtcNow.AddDays(-1));

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Demo data seeding failed. Details: {ex.Message}");
        }
    }

    private static async Task RemoveLegacyDemoDataAsync(ApplicationDbContext context, int teacherId)
    {
        var legacyAssessmentTitles = new[]
        {
            "General Mathematics Readiness Quiz",
            "Earth and Life Science Quiz",
            "Oral Communication Check",
            "Empowerment Technologies Quiz",
            "Practical Research 2 Quiz",
            "Media and Information Literacy Quiz",
            "Entrepreneurship Quiz",
            "Contemporary Philippine Arts Quiz"
        };

        var legacyAssessmentIds = await context.Assessments
            .Where(a => legacyAssessmentTitles.Contains(a.Title))
            .Select(a => a.AssessmentId)
            .ToListAsync();

        foreach (var assessmentId in legacyAssessmentIds)
        {
            await RemoveAssessmentGraphAsync(context, assessmentId);
        }

        var legacyCourseTitles = new[]
        {
            "Mathematics for Everyday Problem Solving",
            "Digital Science and Sustainable Communities"
        };

        var legacyCourses = await context.Courses
            .Where(c => c.TeacherId == teacherId && legacyCourseTitles.Contains(c.Title))
            .ToListAsync();

        foreach (var course in legacyCourses)
        {
            var assessmentIds = await context.Assessments
                .Where(a => a.CourseId == course.CourseId)
                .Select(a => a.AssessmentId)
                .ToListAsync();

            foreach (var assessmentId in assessmentIds)
            {
                await RemoveAssessmentGraphAsync(context, assessmentId);
            }

            var lessons = await context.Lessons.Where(l => l.CourseId == course.CourseId).ToListAsync();
            var enrollments = await context.Enrollments.Where(e => e.CourseId == course.CourseId).ToListAsync();

            context.Lessons.RemoveRange(lessons);
            context.Enrollments.RemoveRange(enrollments);
            context.Courses.Remove(course);
            await context.SaveChangesAsync();
        }

        var legacyAnnouncements = await context.Announcements
            .Where(a => a.Title == "Senior High Learning Analytics Demo")
            .ToListAsync();

        if (legacyAnnouncements.Count > 0)
        {
            context.Announcements.RemoveRange(legacyAnnouncements);
            await context.SaveChangesAsync();
        }

        var seniorHighCourseTitles = GetCourseSeeds().Select(c => c.Title).ToList();
        var legacyLessons = await context.Lessons
            .Include(l => l.Course)
            .Where(l =>
                l.Course != null &&
                l.Course.TeacherId == teacherId &&
                seniorHighCourseTitles.Contains(l.Course.Title) &&
                (l.Title.EndsWith(": Core Concepts") ||
                 l.Title.EndsWith(": Guided Practice") ||
                 l.Title.EndsWith(": Performance Task")))
            .ToListAsync();

        if (legacyLessons.Count > 0)
        {
            context.Lessons.RemoveRange(legacyLessons);
            await context.SaveChangesAsync();
        }
    }

    private static async Task RemoveAssessmentGraphAsync(ApplicationDbContext context, int assessmentId)
    {
        var submissionIds = await context.Submissions
            .Where(s => s.AssessmentId == assessmentId)
            .Select(s => s.SubmissionId)
            .ToListAsync();

        if (submissionIds.Count > 0)
        {
            var answers = await context.StudentAnswers
                .Where(a => submissionIds.Contains(a.SubmissionId))
                .ToListAsync();

            var submissions = await context.Submissions
                .Where(s => submissionIds.Contains(s.SubmissionId))
                .ToListAsync();

            context.StudentAnswers.RemoveRange(answers);
            context.Submissions.RemoveRange(submissions);
        }

        var questions = await context.Questions
            .Where(q => q.AssessmentId == assessmentId)
            .ToListAsync();

        var assessment = await context.Assessments.FirstOrDefaultAsync(a => a.AssessmentId == assessmentId);

        context.Questions.RemoveRange(questions);

        if (assessment != null)
        {
            context.Assessments.Remove(assessment);
        }

        await context.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        ApplicationDbContext context,
        IPasswordHasher<ApplicationUser> passwordHasher,
        string fullName,
        string email,
        UserRole role,
        string password)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                FullName = fullName,
                Email = email,
                Role = role,
                IsActive = true,
                DateCreated = DateTime.UtcNow
            };
            user.PasswordHash = passwordHasher.HashPassword(user, password);
            context.Users.Add(user);
            return user;
        }

        user.FullName = fullName;
        user.Role = role;
        user.IsActive = true;

        var passwordResult = string.IsNullOrWhiteSpace(user.PasswordHash)
            ? PasswordVerificationResult.Failed
            : passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (passwordResult == PasswordVerificationResult.Failed ||
            passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, password);
        }

        return user;
    }

    private static async Task<Course> EnsureCourseAsync(
        ApplicationDbContext context,
        int teacherId,
        SeniorHighCourseSeed seed,
        int courseIndex)
    {
        var course = await context.Courses.FirstOrDefaultAsync(c => c.Title == seed.Title && c.TeacherId == teacherId);

        if (course == null)
        {
            course = new Course
            {
                Title = seed.Title,
                TeacherId = teacherId,
                DateCreated = DateTime.UtcNow.AddDays(-(30 + courseIndex))
            };
            context.Courses.Add(course);
        }

        course.Description = seed.Description;
        course.Subject = seed.Subject;
        course.GradeLevel = seed.GradeLevel;
        await context.SaveChangesAsync();
        return course;
    }

    private static async Task EnsureLessonAsync(
        ApplicationDbContext context,
        int courseId,
        LessonSeed seed,
        int courseIndex)
    {
        var lesson = await context.Lessons.FirstOrDefaultAsync(l => l.CourseId == courseId && l.Title == seed.Title);

        if (lesson == null)
        {
            lesson = new Lesson
            {
                CourseId = courseId,
                Title = seed.Title,
                DateCreated = DateTime.UtcNow.AddDays(-(20 + courseIndex))
            };
            context.Lessons.Add(lesson);
        }

        lesson.Content = seed.Content;
        lesson.IsAvailableOffline = seed.IsAvailableOffline;
        await context.SaveChangesAsync();
    }

    private static async Task<Assessment> EnsureAssessmentAsync(
        ApplicationDbContext context,
        int courseId,
        AssessmentSeed seed)
    {
        var assessment = await context.Assessments.FirstOrDefaultAsync(a => a.CourseId == courseId && a.Title == seed.Title);

        if (assessment == null)
        {
            assessment = new Assessment
            {
                CourseId = courseId,
                Title = seed.Title
            };
            context.Assessments.Add(assessment);
        }

        assessment.Instructions = seed.Instructions;
        assessment.DueDate = DateTime.UtcNow.Date.AddDays(seed.DueInDays).AddHours(23).AddMinutes(59);
        assessment.TotalPoints = 50;
        assessment.IsAvailableOffline = seed.IsAvailableOffline;
        await context.SaveChangesAsync();
        return assessment;
    }

    private static async Task EnsureQuestionAsync(
        ApplicationDbContext context,
        int assessmentId,
        QuestionSeed seed)
    {
        var question = await context.Questions.FirstOrDefaultAsync(q => q.AssessmentId == assessmentId && q.QuestionText == seed.Text);

        if (question == null)
        {
            question = new Question
            {
                AssessmentId = assessmentId,
                QuestionText = seed.Text
            };
            context.Questions.Add(question);
        }

        question.OptionA = seed.OptionA;
        question.OptionB = seed.OptionB;
        question.OptionC = seed.OptionC;
        question.OptionD = seed.OptionD;
        question.CorrectAnswer = seed.CorrectAnswer;
        question.Points = 10;
        await context.SaveChangesAsync();
    }

    private static async Task EnsureEnrollmentAsync(
        ApplicationDbContext context,
        int courseId,
        int studentId,
        int completionPercentage)
    {
        var enrollment = await context.Enrollments.FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);

        if (enrollment == null)
        {
            enrollment = new Enrollment
            {
                CourseId = courseId,
                StudentId = studentId,
                EnrollmentDate = DateTime.UtcNow.AddDays(-18)
            };
            context.Enrollments.Add(enrollment);
        }

        enrollment.CompletionPercentage = completionPercentage;
        await context.SaveChangesAsync();
    }

    private static async Task EnsureSubmissionAsync(
        ApplicationDbContext context,
        int assessmentId,
        int studentId,
        int correctAnswerCount,
        DateTime submittedAt,
        SubmissionStatus status)
    {
        var questions = await context.Questions
            .Where(q => q.AssessmentId == assessmentId)
            .OrderBy(q => q.QuestionId)
            .ToListAsync();

        correctAnswerCount = Math.Clamp(correctAnswerCount, 0, questions.Count);
        var totalPoints = questions.Sum(q => (decimal)q.Points);
        var earnedPoints = questions.Take(correctAnswerCount).Sum(q => (decimal)q.Points);
        var score = totalPoints == 0 ? 0 : Math.Round(earnedPoints / totalPoints * 100, 2);

        var submission = await context.Submissions
            .FirstOrDefaultAsync(s => s.AssessmentId == assessmentId && s.StudentId == studentId);

        if (submission == null)
        {
            submission = new Submission
            {
                AssessmentId = assessmentId,
                StudentId = studentId
            };
            context.Submissions.Add(submission);
        }

        submission.Score = score;
        submission.SubmittedAt = submittedAt;
        submission.Status = status;
        await context.SaveChangesAsync();

        var existingAnswers = await context.StudentAnswers
            .Where(a => a.SubmissionId == submission.SubmissionId)
            .ToListAsync();

        if (existingAnswers.Count > 0)
        {
            context.StudentAnswers.RemoveRange(existingAnswers);
            await context.SaveChangesAsync();
        }

        context.StudentAnswers.AddRange(questions.Select((question, index) =>
        {
            var isCorrect = index < correctAnswerCount;

            return new StudentAnswer
            {
                SubmissionId = submission.SubmissionId,
                QuestionId = question.QuestionId,
                SelectedAnswer = isCorrect ? question.CorrectAnswer : GetIncorrectAnswer(question.CorrectAnswer),
                IsCorrect = isCorrect,
                PointsEarned = isCorrect ? question.Points : 0
            };
        }));

        await context.SaveChangesAsync();
    }

    private static async Task EnsureAnnouncementAsync(
        ApplicationDbContext context,
        string title,
        string message,
        int createdById,
        DateTime createdAt)
    {
        var announcement = await context.Announcements.FirstOrDefaultAsync(a => a.Title == title);

        if (announcement == null)
        {
            announcement = new Announcement
            {
                Title = title
            };
            context.Announcements.Add(announcement);
        }

        announcement.Message = message;
        announcement.CreatedById = createdById;
        announcement.CreatedAt = createdAt;
        await context.SaveChangesAsync();
    }

    private static async Task EnsureUserSettingsAsync(ApplicationDbContext context, int userId)
    {
        if (await context.UserSettings.AnyAsync(s => s.UserId == userId))
        {
            return;
        }

        context.UserSettings.Add(UserSettings.CreateDefault(userId));
        await context.SaveChangesAsync();
    }

    private static IReadOnlyList<SeniorHighCourseSeed> GetCourseSeeds()
    {
        return new List<SeniorHighCourseSeed>
        {
            new(
                "General Mathematics",
                "Mathematics",
                "Grade 11",
                "Functions, simple interest, logic, and real-life quantitative decision making for Senior High learners.",
                "functions and financial decisions",
                "using equations, tables, and graphs to compare practical options",
                "budgeting a school event with clear assumptions and calculations"),
            new(
                "Earth and Life Science",
                "Science",
                "Grade 11",
                "Earth systems, biodiversity, and life processes connected to community sustainability.",
                "Earth systems and biodiversity",
                "explaining how geosphere, hydrosphere, atmosphere, and biosphere interact",
                "planning a school awareness poster about ecosystem protection"),
            new(
                "Oral Communication",
                "Communication",
                "Grade 11",
                "Speech context, audience awareness, and clear communication for academic and civic participation.",
                "audience, purpose, and speech context",
                "choosing language, tone, and evidence for a specific audience",
                "delivering a short speech that uses active listening and respectful feedback"),
            new(
                "Empowerment Technologies",
                "ICT",
                "Grade 11",
                "Digital productivity, online collaboration, media creation, and responsible technology use.",
                "responsible digital productivity",
                "creating shared online materials that are organized, accessible, and credible",
                "building an offline-ready learning resource for classmates"),
            new(
                "Practical Research 2",
                "Research",
                "Grade 12",
                "Quantitative research design, data collection, analysis, and evidence-based conclusions.",
                "quantitative research design",
                "turning measurable problems into variables, tools, data, and conclusions",
                "presenting survey results that support a school improvement recommendation"),
            new(
                "Media and Information Literacy",
                "Media Literacy",
                "Grade 12",
                "Critical evaluation of media, information sources, digital citizenship, and responsible sharing.",
                "credible information evaluation",
                "checking authorship, evidence, bias, date, and purpose before sharing media",
                "creating a fact-check guide for classmates"),
            new(
                "Entrepreneurship",
                "Business",
                "Grade 12",
                "Opportunity recognition, customer needs, value proposition, and sustainable enterprise planning.",
                "value proposition and customer needs",
                "testing whether a product idea solves a real community problem",
                "drafting a simple business model canvas for a student-led service"),
            new(
                "Contemporary Philippine Arts",
                "Arts",
                "Grade 12",
                "Regional art forms, cultural identity, creative production, and contemporary expression.",
                "regional art and cultural identity",
                "analyzing how contemporary works reflect local stories, materials, and communities",
                "curating a short exhibit note for a regional artwork")
        };
    }

    private static IReadOnlyList<LessonSeed> BuildLessons(SeniorHighCourseSeed course, int courseIndex)
    {
        var lectureTitles = GetLectureTitles(course.Title);

        return new List<LessonSeed>
        {
            new(
                lectureTitles[0],
                $"This lecture introduces {course.CoreConcept} for {course.GradeLevel} learners. Students define key terms, study a guided example, and connect the topic to quality education through practical classroom participation.",
                true),
            new(
                lectureTitles[1],
                $"Students practice {course.ClassroomApplication}. The activity includes a short teacher demonstration, pair work, and a checkpoint question that helps identify learners who need additional support.",
                false),
            new(
                lectureTitles[2],
                $"Students complete a practical task: {course.PerformanceTask}. The output can be reviewed online or offline and contributes to the course completion progress shown in the learner dashboard.",
                courseIndex % 2 == 0)
        };
    }

    private static IReadOnlyList<string> GetLectureTitles(string courseTitle)
    {
        return courseTitle switch
        {
            "General Mathematics" => new[] { "Functions and Relations", "Simple and Compound Interest", "Basic Statistics" },
            "Earth and Life Science" => new[] { "Earth Systems", "Rocks and Minerals", "Natural Hazards" },
            "Oral Communication" => new[] { "Communication Process", "Verbal and Nonverbal Communication", "Public Speaking Basics" },
            "Empowerment Technologies" => new[] { "ICT in Education", "Online Safety and Netiquette", "Productivity Tools" },
            "Practical Research 2" => new[] { "Research Design", "Data Gathering Procedure", "Data Analysis and Interpretation" },
            "Media and Information Literacy" => new[] { "Media Literacy Basics", "Evaluating Online Sources", "Responsible Media Use" },
            "Entrepreneurship" => new[] { "Business Ideas and Opportunities", "Marketing Plan Basics", "Financial Planning" },
            "Contemporary Philippine Arts" => new[] { "Philippine Art Forms", "Regional Artists", "Contemporary Art Appreciation" },
            _ => new[] { $"{courseTitle} Overview", $"{courseTitle} Guided Practice", $"{courseTitle} Performance Task" }
        };
    }

    private static IReadOnlyList<AssessmentSeed> BuildAssessments(SeniorHighCourseSeed course, int courseIndex)
    {
        return new List<AssessmentSeed>
        {
            new(
                $"{course.Title} Foundations Quiz",
                $"Choose the best answer. This quiz checks readiness in {course.CoreConcept}.",
                7 + courseIndex,
                true,
                true,
                course.CoreConcept,
                course.ClassroomApplication),
            new(
                $"{course.Title} Application Quiz",
                $"Choose the best answer. This quiz checks how well students can apply the lesson through {course.PerformanceTask}.",
                21 + courseIndex,
                courseIndex % 2 == 0,
                false,
                course.PerformanceTask,
                course.ClassroomApplication)
        };
    }

    private static IReadOnlyList<QuestionSeed> BuildQuestions(SeniorHighCourseSeed course, AssessmentSeed assessment)
    {
        return new List<QuestionSeed>
        {
            new(
                $"Which idea best describes {assessment.Focus} in {course.Title}?",
                assessment.Focus,
                "a disconnected classroom decoration",
                "a random attendance code",
                "a task with no learning purpose",
                "A"),
            new(
                $"What action shows strong learning practice in {course.Title}?",
                assessment.Application,
                "submitting without checking the work",
                "copying answers without understanding",
                "avoiding feedback from classmates",
                "A"),
            new(
                $"How can teachers use results from {course.Title} quizzes?",
                "identify learners who need follow-up support",
                "hide all performance information",
                "remove lessons from the course",
                "ignore missing submissions",
                "A"),
            new(
                $"Which output best demonstrates understanding of {course.Title}?",
                "a clear answer supported by evidence or examples",
                "a blank response",
                "an unrelated drawing only",
                "a copied definition with no explanation",
                "A"),
            new(
                $"Why is {course.Title} important for Senior High School learners?",
                "it connects classroom learning with real decisions and community needs",
                "it prevents students from reviewing lessons",
                "it removes the need for teacher feedback",
                "it makes offline learning impossible",
                "A")
        };
    }

    private static int GetCompletionPercentage(ApplicationUser student, int courseIndex)
    {
        if (string.Equals(student.Email, StudentEmail, StringComparison.OrdinalIgnoreCase))
        {
            return Math.Min(95, 68 + (courseIndex * 4));
        }

        if (student.Email.Contains("student2", StringComparison.OrdinalIgnoreCase))
        {
            return 52 + (courseIndex % 3 * 6);
        }

        if (student.Email.Contains("student3", StringComparison.OrdinalIgnoreCase))
        {
            return 70 + (courseIndex % 4 * 5);
        }

        return 60 + (courseIndex % 5 * 7);
    }

    private static int GetCorrectAnswerCount(ApplicationUser student, int courseIndex, int assessmentIndex)
    {
        if (string.Equals(student.Email, StudentEmail, StringComparison.OrdinalIgnoreCase))
        {
            return courseIndex % 3 == 0 ? 5 : 4;
        }

        if (student.Email.Contains("student2", StringComparison.OrdinalIgnoreCase))
        {
            return courseIndex % 2 == 0 ? 3 : 4;
        }

        if (student.Email.Contains("student3", StringComparison.OrdinalIgnoreCase))
        {
            return courseIndex % 4 == 0 ? 5 : 4;
        }

        return ((student.Id + courseIndex + assessmentIndex) % 3) switch
        {
            0 => 5,
            1 => 4,
            _ => 3
        };
    }

    private static SubmissionStatus GetSubmissionStatus(ApplicationUser student, SeniorHighCourseSeed course)
    {
        if (string.Equals(student.Email, StudentEmail, StringComparison.OrdinalIgnoreCase) &&
            course.Title == "Media and Information Literacy")
        {
            return SubmissionStatus.PendingSync;
        }

        return SubmissionStatus.Synced;
    }

    private static string GetIncorrectAnswer(string correctAnswer)
    {
        return correctAnswer switch
        {
            "A" => "B",
            "B" => "C",
            "C" => "D",
            _ => "A"
        };
    }

    private sealed record SeniorHighCourseSeed(
        string Title,
        string Subject,
        string GradeLevel,
        string Description,
        string CoreConcept,
        string ClassroomApplication,
        string PerformanceTask);

    private sealed record LessonSeed(
        string Title,
        string Content,
        bool IsAvailableOffline);

    private sealed record AssessmentSeed(
        string Title,
        string Instructions,
        int DueInDays,
        bool IsAvailableOffline,
        bool CreateDemoSubmissions,
        string Focus,
        string Application);

    private sealed record QuestionSeed(
        string Text,
        string OptionA,
        string OptionB,
        string OptionC,
        string OptionD,
        string CorrectAnswer);
}
