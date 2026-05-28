# EduTrack

EduTrack is a simple learning management and student performance monitoring system made with ASP.NET Core MVC.

This project is focused on **Senior High School students**, especially **Grade 11 and Grade 12**. It helps teachers manage lessons, assessments, student scores, reports, and learning materials. It also includes dashboards for admin, teachers, and students.

The system was made for our school project and is connected to **SDG 4: Quality Education**.

---

## What the System Does

EduTrack allows:

* Teachers to add courses, lessons, assessments, and learning materials.
* Students to view lessons, take assessments, check scores, and see their progress.
* Admins to monitor users, courses, reports, and overall system data.
* Users to switch between light mode and dark mode.
* Teachers to upload PDF, Word, PowerPoint, or text files for lessons.
* Students to access offline-ready lessons and learning materials.
* The system to show reports, charts, and performance labels.

---

## Why It Supports SDG 4

SDG 4 is about quality education. EduTrack supports this by helping teachers monitor student progress and identify students who may need help.

The system also supports offline learning, which is useful when students have unstable internet connection.

---

## Demo Accounts

Use these accounts to test the system:

| Role    | Email                                                   | Password    |
| ------- | ------------------------------------------------------- | ----------- |
| Admin   | [admin@edutrack.local](mailto:admin@edutrack.local)     | Admin123!   |
| Teacher | [teacher@edutrack.local](mailto:teacher@edutrack.local) | Teacher123! |
| Student | [student@edutrack.local](mailto:student@edutrack.local) | Student123! |

There are also extra sample student accounts included for testing class analytics and reports.

---

## Grade Level Scope

The demo system focuses only on:

* Grade 11
* Grade 12

Sample Grade 11 courses:

* General Mathematics
* Earth and Life Science
* Oral Communication
* Empowerment Technologies

Sample Grade 12 courses:

* Practical Research 2
* Media and Information Literacy
* Entrepreneurship
* Contemporary Philippine Arts

---

## Main Features

### Admin

Admin can:

* View system dashboard
* Manage users
* View courses
* View reports
* Manage announcements
* Monitor overall performance

### Teacher

Teacher can:

* View teacher dashboard
* Manage courses
* Add lectures or lessons
* Upload learning materials
* Create assessments
* View class list
* Monitor student progress
* View students needing support
* Generate reports
* Post announcements

### Student

Student can:

* View enrolled courses
* Read lessons
* Download learning materials
* Take assessments
* View scores
* Check progress
* View reports
* Access offline learning materials
* Read announcements

---

## Learning Material Upload

Teachers can upload files when adding or editing a lesson.

Allowed file types:

* PDF
* Word document
* PowerPoint
* Text file

Maximum file size:

```text
10 MB
```

Uploaded files are saved in:

```text
wwwroot/uploads/lessons
```

Students can open or download the uploaded learning materials from the lesson page.

---

## Offline Learning

Some lessons can be marked as **Available Offline**.

When a lesson is marked offline-ready, students will see an **Available Offline** badge. If the lesson has an uploaded file, students should download it while online so they can access it later.

The system also has an online/offline status indicator. The online message only appears briefly, while the offline message appears when the browser loses connection.

---

## Reports and Analytics

The system includes simple reports and charts such as:

* Student performance report
* Class performance report
* Course completion report
* Assessment result report
* Course progress charts
* Pass/fail summaries
* Students needing support

Reports can be printed.

---

## Appearance Settings

Users can change basic appearance settings such as:

* Light Mode
* Dark Mode
* Font Size
* Layout Style

These settings help make the system easier to read and use.

---

## Technologies Used

* ASP.NET Core MVC
* Entity Framework Core
* SQL Server / SQL Express
* Code-First Migrations
* HTML
* CSS
* JavaScript
* Bootstrap
* Service Worker
* Web App Manifest

---

## How to Run the Project

Open a terminal inside the project folder.

Restore packages:

```powershell
dotnet restore
```

Build the project:

```powershell
dotnet build
```

Apply database migrations:

```powershell
dotnet ef database update
```

Run the system:

```powershell
dotnet run --urls http://localhost:5107
```

Open this in the browser:

```text
http://localhost:5107
```

---

## Database Connection

The project uses SQL Server / LocalDB for local development.

The default connection string is in:

```text
appsettings.Development.json
```

Default example for a clean clone:

```json
"Server=(localdb)\\mssqllocaldb;Database=EduTrackAnalyticsDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
```

If your computer uses SQL Express instead of LocalDB, update the connection string before running the database migration. Example:

```json
"Server=YOUR-PC\\SQLEXPRESS;Database=EduTrackAnalyticsDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
```

---

## How to Test the System

After running the app, test the system using the demo accounts.

Suggested testing flow:

1. Open the homepage.
2. Login as Admin.
3. Check the admin dashboard and reports.
4. Logout.
5. Login as Teacher.
6. Add a lecture or lesson.
7. Upload a learning material.
8. Mark the lesson as Available Offline.
9. Create or check assessments.
10. View class list and student progress.
11. Logout.
12. Login as Student.
13. Check lessons.
14. Open or download the uploaded material.
15. Take an assessment.
16. View scores and reports.
17. Test dark mode.
18. Test logout.

---

## If Something Goes Wrong

If the site says:

```text
localhost refused to connect
```

make sure the app is still running in the terminal.

Run again:

```powershell
dotnet run --urls http://localhost:5107
```

If the database is empty or missing data, run:

```powershell
dotnet ef database update
```

Then restart the app.

If `/Account/Register` shows **ERROR 500** or says registration is temporarily unavailable, the database is usually not migrated yet or the SQL Server connection string does not match the computer. Run:

```powershell
dotnet ef database update
```

Then check `appsettings.Development.json` and confirm the `DefaultConnection` points to an available SQL Server / LocalDB instance.

If CSS or dark mode changes do not appear, do a hard refresh:

```text
Ctrl + F5
```

If the browser is using old cached files, unregister the service worker from browser developer tools.

---

## Reset Demo Data

To reset the database during development:

```powershell
dotnet ef database drop
dotnet ef database update
dotnet run --urls http://localhost:5107
```

After running again, the demo data will be seeded automatically.

---

## GitHub Notes

Before pushing to GitHub, make sure these folders are not included:

```text
bin/
obj/
.vs/
logs/
local database files
```

To push changes:

```powershell
git status
git add .
git commit -m "Update EduTrack project"
git push
```

---

## Folder Overview

```text
/Controllers
Contains the controllers for each main feature.

/Models
Contains the database models.

/ViewModels
Contains data used by views and dashboards.

/Views
Contains the pages of the system.

/Data
Contains the database context and seed data.

/Services
Contains helper services for analytics and labels.

/Migrations
Contains Entity Framework Core migrations.

/wwwroot
Contains CSS, JavaScript, images, uploaded files, manifest, and service worker.
```

---

## Project Summary

EduTrack is a school project built to help manage learning, assessments, and student performance. It is designed for Grade 11 and Grade 12 students and includes features for admin, teachers, and students.

The main goal of the system is to make learning progress easier to track and to help teachers identify students who may need support.
