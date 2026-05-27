# EduTrack

Online and Offline Learning Management and Student Performance Monitoring System

## Description

EduTrack is an ASP.NET Core MVC web application aligned with SDG 4: Quality Education. It supports Senior High School course management, lesson delivery, online assessments, offline assessment storage, student performance monitoring, analytics dashboards, announcements, enrollments, appearance settings, and printable reports.

The demo setup focuses on Grade 11 and Grade 12 learners so the system shows meaningful school-project presentation data immediately after startup.

## SDG 4 Relevance

SDG 4 promotes inclusive and equitable quality education. EduTrack supports this by:

- Helping teachers identify struggling learners through performance analytics.
- Allowing students to continue reviewing lessons and answering assessments during low-connectivity periods.
- Giving administrators school-wide visibility into courses, assessments, users, reports, and pass/fail summaries.
- Supporting accessible learning continuity through Progressive Web App offline features and readable appearance settings.

## Senior High School Scope

The main demo is focused on Grade 11 and Grade 12 learners.

Grade 11 demo courses:

- General Mathematics
- Earth and Life Science
- Oral Communication
- Empowerment Technologies

Grade 12 demo courses:

- Practical Research 2
- Media and Information Literacy
- Entrepreneurship
- Contemporary Philippine Arts

The grade filter intentionally shows only All grades, Grade 11, and Grade 12.

## Demo Login Accounts

| Role | Email | Password |
| --- | --- | --- |
| Admin | admin@edutrack.local | Admin123! |
| Teacher | teacher@edutrack.local | Teacher123! |
| Student | student@edutrack.local | Student123! |

Extra sample student accounts are seeded for class analytics and reports, including Jastin Percunod and Kurt Recto. Existing active student accounts are also enrolled in the demo courses.

## System Roles

- Admin: manage users, courses, enrollments, announcements, reports, and system-wide analytics.
- Teacher: manage courses, lessons, assessments, questions, submissions, announcements, and class analytics.
- Student: view enrolled courses, lessons, assessments, scores, offline library, announcements, settings, and reports.

## Demo Data Included

The seed data is idempotent, so it updates existing demo records instead of duplicating them every time the app runs.

Each course includes:

- 3 lessons
- At least 1 offline-ready lesson
- 2 assessments
- 5 multiple-choice questions per assessment
- Future assessment due dates
- Completed submissions and scores for demo dashboards
- Enrollments and completion data for reports

Announcements included:

- Welcome to EduTrack Analytics
- Upcoming Grade 11 and Grade 12 assessments
- Offline learning materials are available

## User-Friendly Improvements

- Performance labels: Excellent, Good, Needs Improvement, and At Risk.
- Students Needing Support section for teachers.
- Offline Learning Library for lessons and assessments.
- Auto-score feedback with score, percentage, passed/failed status, performance label, and recommendation.
- Better empty-state guidance across courses, lessons, assessments, scores, and reports.
- Teacher quick actions: Create Course, Add Lesson, Create Assessment, View Reports, and Post Announcement.
- Notification-style dashboard messages for assessments, lessons, scores, announcements, and offline sync.

## Appearance Settings

All logged-in users can open Settings > Appearance Settings.

Available options:

- Theme Mode: Light Mode, Dark Mode, System Default
- Layout Style: Comfortable Layout, Compact Layout
- Sidebar Preference: Expanded Sidebar, Collapsed Sidebar
- Font Size: Small, Medium, Large
- Dashboard Card Style: Default cards, Minimal cards

Settings are saved per user in the database and also stored in localStorage so the selected appearance loads quickly and still works while offline.

## Chart and Analytics Improvements

Dashboard charts were improved for presentation readability:

- Student Dashboard: Course Progress Overview, Recent Assessment Scores, Completion Status.
- Teacher Dashboard: Class Average by Course, Students Needing Support, Assessment Submission Status.
- Admin Dashboard: Overall Student Performance, Course Enrollment Summary, Pass/Fail Distribution.
- Course labels are shortened on charts, with full labels available in hover tooltips.
- Horizontal bar charts are used for course progress and averages to prevent overlapping course names.
- Charts respond to Light Mode and Dark Mode colors.
- Empty chart messages appear when data is not available.

## Technologies Used

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core
- SQL Server / SQL Server Express / SQL Server LocalDB
- InMemory development fallback for Codex sandbox environments without SQL Server
- Code-First Migrations
- Cookie authentication with role-based authorization
- HTML, CSS, Bootstrap, JavaScript
- Service Worker, Web App Manifest, LocalStorage

## How to Run

1. Install the .NET 10 SDK.
2. Install SQL Server Express or SQL Server LocalDB for normal development.
3. Open a terminal in this project folder.
4. Restore packages:

```powershell
dotnet restore
```

5. Build the project:

```powershell
dotnet build
```

6. Check the development connection string in `appsettings.Development.json`.

Default local SQL Express connection:

```json
"Server=KehnPC\\SQLEXPRESS;Database=EduTrackAnalyticsDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
```

7. Apply the migrations:

```powershell
dotnet ef database update
```

If `dotnet ef` is not installed:

```powershell
dotnet tool install --global dotnet-ef
dotnet ef database update
```

8. Run the application:

```powershell
dotnet run --urls http://localhost:5107
```

9. Open in browser:

```text
http://localhost:5107
```

## How to Test the Demo

1. Open `http://localhost:5107`.
2. Login with each demo account.
3. Confirm each dashboard shows non-empty statistics and readable charts.
4. Open Courses and test All grades, Grade 11, and Grade 12 filters.
5. Open Lessons and check the Available Offline labels.
6. Open Assessments and verify upcoming and completed assessments appear.
7. Open Scores and verify course name, assessment title, score, total points, percentage, performance label, status, and submitted date.
8. Open Reports and use the Print button.
9. Open Settings, test Light Mode, Dark Mode, System Default, compact layout, sidebar state, font sizes, and card style.
10. Refresh the browser and confirm saved settings remain.
11. Disconnect internet or use browser offline mode to check the offline banner and cached pages.

## Printing Reports

Reports include a title, date generated, summary statistics, table data, and a Print button. Print CSS forces a clean light background even when Dark Mode is enabled.

## Reset or Reseed Demo Data

The app reseeds missing or changed demo records automatically on startup. To fully reset the database during development:

```powershell
dotnet ef database drop
dotnet ef database update
dotnet run --urls http://localhost:5107
```

After startup, login with the demo accounts above. Existing active student accounts are automatically enrolled in the Senior High School demo courses.

## Online and Offline Usage

- The app includes `wwwroot/manifest.json` and `wwwroot/service-worker.js`.
- The service worker caches public pages, login, CSS, JavaScript, images, and the offline page.
- Lessons marked Available Offline can be saved locally from the lesson details page.
- The Offline Learning Library displays Available Offline, Saved for Offline Use, and Needs Internet labels.
- Assessments marked Available Offline can store answers locally when the browser is offline.
- When the browser returns online, pending submissions are sent to `/Assessments/SyncOffline`.
- The top banner changes between Online Mode and Offline Mode.
- The offline page is available at `/Home/Offline`.
- Appearance settings are stored in localStorage so the selected theme can still apply while offline.

## Troubleshooting

- If `dotnet ef` is not found, install it with `dotnet tool install --global dotnet-ef`.
- If SQL Server cannot connect, confirm SQL Express is running and that `appsettings.Development.json` uses the correct server name.
- If the database looks empty, run `dotnet ef database update`, then restart the app so `SeedData` can reseed demo records.
- If a port is already in use, change the run URL, for example `dotnet run --urls http://localhost:5110`.
- If static files do not update, hard refresh the browser or unregister the service worker from browser dev tools.
- Do not upload `bin/`, `obj/`, `.vs/`, logs, local database files, or local production settings to GitHub.

## GitHub Submission Notes

The repository should include source files, migrations, views, controllers, models, services, `wwwroot`, `README.md`, `.gitignore`, and `EduTrack.sln`.

Recommended GitHub commands:

```powershell
git init
git add .
git commit -m "Finalize EduTrack Analytics MVC project for submission"
git branch -M main
git remote add origin https://github.com/kehnrev/PU-LEARNING-MANAGEMENT.git
git push -u origin main
```

Only add the remote if that repository exists and you have permission to push.

## Folder Structure

```text
/Controllers
  HomeController, AccountController, AdminController, TeacherController, StudentController
  CoursesController, LessonsController, AssessmentsController, QuestionsController
  SubmissionsController, AnnouncementsController, EnrollmentsController, ReportsController
  SettingsController

/Models
  ApplicationUser, UserSettings, Course, Lesson, Assessment, Question, Submission
  StudentAnswer, Announcement, Enrollment, UserRole, SubmissionStatus

/ViewModels
  Account, dashboard, contact, assessment-taking, report, and settings view models

/Views
  Public website pages, role dashboards, settings page, CRUD pages, report pages, shared layout

/Data
  ApplicationDbContext, SeedData

/Services
  PerformanceAnalyticsService, PerformanceLabelHelper, ChartLabelHelper, UserClaimsExtensions

/Migrations
  Code-First migrations including the UserSettings table

/wwwroot
  CSS, JavaScript, images, manifest, service worker
```
