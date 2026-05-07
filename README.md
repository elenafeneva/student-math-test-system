# Student Math Test Validator

Automated .NET grading system that ingests teacher XML uploads, persists teachers/students/exams/tasks, evaluates math expressions and exposes simple APIs and a minimal WPF client.

## Project layout
- MathTaskValidator.Api — ASP.NET Core Web API (controllers, MediatR features, services).
-	MathTaskValidator.Infrastructure — EF Core AppDbContext and migrations.
-	MathTaskValidator.Core — domain models and result DTOs (entities: Teacher, Student, Exam, ExamTask).
-	MathTaskValidator.App — minimal WPF client (upload UI, student and teacher views).

When running EF CLI commands you must specify `--project` and `--startup-project` as shown in the migrations section. For example to run the migrations for this project ensure that you are in `MathTaskValidator.Api` project and then run `dotnet ef database update --project ../MathTaskValidator.Infrastructure --startup-project ../MathTaskValidator.Api`

---

## Prerequisites
- .NET 8 SDK
- SQL Server instance (LocalDB or full SQL Server)
- Visual Studio 2022/2023 or VS Code
- `dotnet-ef` tool (for migrations)

---

## Configuration
Important configuration files:
- MathTaskValidator.Api
	- `MathTaskValidator.Api/appsettings.json`
	- `MathTaskValidator.Api/appsettings.Development.json`
- MathTaskValidator.App
	- `MathTaskValidator.App/AppSettings.cs`

Important keys used by the app (examples):
- `ConnectionString` 
- `UploadsPath` (optional; default uploads)

---

## Database / Migrations
1.	Install dotnet-ef tool if needed:
-	`dotnet tool install --global dotnet-ef`
2.	Apply migrations:
-	`dotnet ef database update --project MathTaskValidator.Infrastructure --startup-project MathTaskValidator.Api`
-	This applies migrations from the Infrastructure project using the API startup for configuration/resolution.

---

## Run the project 
### Start apps
-	Run `MathTaskValidator.Api` (ensure it’s running).
-	Then run `MathTaskValidator.App (WPF)` — The WPF client calls the running API and reads API base URL from env var API_BASE_URL.
Both must be running concurrently for uploads and queries to work.

### Workflow steps
1.	Ensure DB exists and migrations applied:
-	`dotnet ef database update --project MathTaskValidator.Infrastructure --startup-project MathTaskValidator.Api`
2.	Upload a file (teacher workflow)
-	Open the WPF app → Teacher → enter Teacher Unique ID (this must match the ID attribute in the Teacher element inside the XML).
-	Click “Choose File and Upload” and pick the XML file.
-	On success you’ll get confirmation; uploaded file is saved to the API uploads folder and entities are persisted.
3.	Teacher viewing students/results
-	In the WPF app Teacher view click “View Students Results”.
-	In the teacher results window enter the same Teacher Unique ID and click “Load Students”.
-	You'll get a list of students (unique id) and each student’s exam summary.
4.	Student viewing their results
-	In the WPF app Student view enter the Student Unique ID and press Enter.
-	The app calls `GET /api/students/{id}/results` and displays exam results + per-task details.

---

## Notes
- Math evaluation uses mxparser in `MathTaskValidator.Api.Services.MathCalculation.MathProcessor`
- UploadDataService handles XML parsing, entity creation and file storage.
- StudentService retrieves student data and groups exam results.
- IMathProcessor computes ExamResult and TaskResult.