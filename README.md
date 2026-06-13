# HHMCore LMS

![Build and Test](https://github.com/hajrahmuzammal-glitch/HHMCoreLMS/actions/workflows/build-and-test.yml/badge.svg)

A production-architecture University Learning Management System built with ASP.NET Core 8 Web API — demonstrating layered architecture, role-based security, conflict detection, and engineering discipline across every layer of the stack.

---

## What This Is

HHMCore LMS handles the full administrative side of a university: student and teacher management, room and timetable scheduling, attendance, and grading. It is built as a real system with real constraints, with production standards in mind.

The design decisions are documented with the reasoning behind them. The bugs are documented with root causes and what changed. Both are recorded as they happened, not cleaned up after the fact.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8 Web API |
| Database | SQL Server (SQLEXPRESS) |
| ORM | Entity Framework Core 8 |
| Auth | ASP.NET Core Identity + JWT |
| Validation | FluentValidation 11 |
| Mapping | Manual mapping extension methods |
| Logging | Serilog (console + file) |
| Testing | xUnit + Moq + FluentAssertions |
| Frontend | Angular 21 (standalone components) |
| UI Library | PrimeNG 20 + PrimeFlex |

---

## Architecture

Four-project layered architecture with strict separation of concerns.

```
HHMCoreLMS.sln
├── HHMCore.WebAPI    → Controllers, Middleware, Program.cs
├── HHMCore.Core      → Entities, DTOs, Interfaces, Services, Validators, Mappings
├── HHMCore.Data      → AppDbContext, Repositories, Migrations, Seeders
└── HHMCore.Tests     → xUnit service layer tests
```

Project dependencies:
- `HHMCore.WebAPI` depends on Core + Data
- `HHMCore.Data` depends on Core only
- `HHMCore.Core` depends on nothing

The database layer can be replaced without touching business logic. Business logic has zero knowledge of HTTP or EF Core internals.

---

**How a Request Flows Through This System**

The full execution path of every request, from entry to database:

```
Program.cs            → The day planner. Runs once at startup. Sets everything up.
HTTP Request          → You walking into a company for an interview.
ExceptionMiddleware   → The security guard. Catches any crash professionally.
Authentication        → Reception desk checks your JWT pass card.
Authorization         → They check your role. Not everyone gets every room.
Router                → Receptionist checks which controller handles this request.
Controller            → The interview room. Receives, delegates, sends out. Makes no decisions.
Service               → The actual interviewer. Reviews everything. Makes ALL decisions.
IUnitOfWork           → Decides what gets packaged before anything saves.
UnitOfWork            → Does the actual wrapping. One transaction. All or nothing.
IGenericRepository    → Defines what the filing system can do.
GenericRepository     → Actually performs those actions via EF Core.
AppDbContext          → The person at the PC with the database open.
Database              → The company record. Final destination.
```

---

## Modules

| Module | Status | Endpoints |
|---|---|---|
| Authentication (JWT) | ✅ Complete | 3 |
| Role Management | ✅ Complete | 4 |
| Department | ✅ Complete | 5 |
| Designation | ✅ Complete | 5 |
| Building | ✅ Complete | 5 |
| Room | ✅ Complete | 6 |
| Course | ✅ Complete | 6 |
| Semester | ✅ Complete | 6 |
| Student | ✅ Complete | 6 |
| Teacher | ✅ Complete | 7 |
| Time Slot | ✅ Complete | 5 |
| Schedule (Timetable) | 🔧 In progress | — |
| Attendance | ⬜ Planned | — |
| Grading | ⬜ Planned | — |
| Admin Dashboard | ⬜ Planned | — |


---

## Design Decisions and Corrections

These are the decisions made deliberately upfront and the problems identified and corrected during development. The decisions show architectural thinking.
---

**Manual Mapping over AutoMapper**

The project started with AutoMapper. After working with it across several modules, the decision was made to remove it entirely and replace it with manual mapping extension methods.

AutoMapper reduces boilerplate but creates a debugging surface that is nearly invisible when something goes wrong. In a layered architecture where every data transformation needs to be traceable, that cost outweighs the convenience. Manual mapping is explicit, fully traceable, and directly testable. This decision came from learning to evaluate tools by what they cost, not just what they provide.

---

**Two-DTO Security Pattern**

Each role gets only the DTO it needs. A teacher updating their own profile receives `UpdateTeacherProfileDto` covering phone, address, and qualification only. An admin receives `UpdateTeacherDto` covering salary, department, and designation. Mass assignment is eliminated at the contract level, not just with authorization guards.

The mass assignment vulnerability was caught during design review. It was not in the original structure — it was identified and corrected before the module shipped.

---

**Schedule Module — Conflict Detection**

This was the most heavily redesigned module in the project. The original design used plain strings for room and time slot references. Plain strings accept anything, enable no conflict detection, and create no referential integrity. The design was identified as wrong and corrected: proper foreign keys to `Room` and `TimeSlot` entities replaced the strings, enabling the four-rule conflict detection system that runs before every course assignment saves.

The four rules:

1. **Room conflict** — same room already booked at that time slot in that semester
2. **Teacher conflict** — teacher already assigned to another class at that time
3. **Course conflict** — course already scheduled at that time
4. **Student conflict** — department and semester students already have a class at that time

All four must pass. If any fails, nothing saves, and the response tells you exactly which conflict was detected.

---

**Soft Delete Architecture**

No record is ever hard-deleted. `IsDeleted = true` is set on deletion. A Global Query Filter on every entity automatically excludes deleted records from every query. The filter is applied once in `AppDbContext` — `WHERE IsDeleted = 0` does not appear anywhere else in the codebase.

---

**Role-Based Access Control**

Three roles: Admin, Teacher, Student. Seeded via `DbSeeder`. All role references use an `AppRoles` constants class. The string `"Admin"` does not appear anywhere in the codebase outside that class.

---

**Consistent API Response Envelope**

Every endpoint returns the same wrapper regardless of success or failure:

```json
{
  "success": true,
  "message": "Teacher created successfully.",
  "data": { },
  "errors": null
}
```

[SCREENSHOT: A single clean API response in Swagger — show a 200 response for GET /api/departments or similar, with the JSON body expanded]

The Angular frontend checks `response.success` uniformly without per-endpoint handling.

---

**False-Passing Unit Tests**

During the unit testing phase, tests were passing not because the logic was correct but because the assertions were not verifying the right things. Tests were asserting that methods were called without verifying what they were called with. The false passes were identified, the assertions were corrected to verify both the call and the exact arguments, and the test suite now genuinely covers the service layer behavior it claims to cover.

---

**Git Workflow Corrected Mid-Project**

Commit discipline was identified as not matching professional standards partway through development. Rather than continuing, development stopped, Conventional Commits was adopted, and the workflow was restructured going forward.

---

**System Boundary Decision**

HHMCore LMS is a post-admission system. Students and teachers are already enrolled or hired when they appear in this system. Pre-admission flows belong in a separate application portal. This boundary kept the LMS focused and the data model clean.

---

## Production Hardening

Production concerns addressed in the current codebase:

| Concern | Implementation |
|---|---|
| Read-only query performance | `AsNoTracking()` on all queries that do not result in an update |
| Soft delete safety | Global Query Filter on `IsDeleted` applied once in `AppDbContext` — cannot be forgotten |
| Referential integrity | `DeleteBehavior.Restrict` on all foreign keys — no accidental cascade deletes |
| Decimal precision | `decimal(18,2)` explicitly configured on every decimal column |
| Null safety | `<Nullable>enable</Nullable>` in all three projects — null issues caught at build time |
| Code consistency | `.editorconfig` enforced at solution level |
| CI | GitHub Actions runs build and test on every push — badge above reflects current state |

---

## Actively Evolving

These are the next production concerns being addressed, in sequence:

**`CancellationToken` across all async methods.** Without this, if a user navigates away or a request is abandoned, the original database query keeps running and consuming connection pool resources. With it, the operation cancels cleanly the moment the HTTP connection drops. This is being propagated from controller through service to every EF Core call.

**`ILogger` with structured logging in every service.** When something breaks in production, structured logs let you filter by entity ID, user, or operation and trace exactly what happened and when. String interpolation in logs loses that searchability. Every service is getting an injected `ILogger` with named parameters on every log call.

**`sealed` on all service and validator implementations** — prevents unintended inheritance and enables JIT optimizations on method dispatch.

**`ExceptionMiddleware`** updated to return semantic HTTP status codes matched to exception type rather than returning 500 for everything.

**`appsettings.Production.json`** with warning-level logging. Information-level logs running against real traffic fill disk fast.

**JWT secret key via environment variable.** No secrets in source control.

**NetArchTest architecture tests** added to CI. Layer separation enforced automatically on every push — Core referencing Data becomes a failing build, not a code review comment.

**Database-level uniqueness enforcement** via unique indexes as a second layer behind service-level duplicate checks.

---

## Unit Tests

Tests live in `HHMCore.Tests` and cover the service layer exclusively. Controllers are thin by design — all business logic lives in services, and that is where the tests are.

**Tools:** xUnit, Moq, FluentAssertions

**Pattern:** Arrange-Act-Assert throughout. Every test has one reason to exist and one thing it verifies.

**What is covered:** Every service method across create, read, update, delete, and all conflict detection paths. Both the happy path and all documented failure paths are tested. A service method with three ways to fail has three tests for those failures.

**Two non-trivial examples worth noting:**

The CourseAssignment conflict detection tests use `SetupSequence` on the mocked repository because `ExistsAsync` is called four times in sequence, each returning a different value — one per conflict rule. A single `Setup` returning the same value every time would not test the real behavior.

`MapperFactory` is used across tests instead of a mocked mapper. Mocking the mapper produces tests that pass regardless of whether the mapping is correct. A real mapper instance with the actual profile registered catches mapping bugs that a mock would never surface.

---

## Project Rules

Enforced in every pull request:

| Rule | Reason |
|---|---|
| PUT request body never contains an `Id` field — ID comes from the route only | Prevents ambiguity between route and body; eliminates a class of silent bugs |
| After POST and PUT, navigation properties are always re-fetched with `.Include()` before mapping the response | Null fields in API responses are a bug, not a design choice |
| Duplicate and conflict checks happen in the service layer before any insert | Controllers are not the place for business decisions |
| Business logic never appears in controllers | Controllers receive, delegate, and respond — nothing else |
| `decimal` columns always use `HasColumnType("decimal(18,2)")` | EF Core default precision loses data silently |
| All cascade deletes use `DeleteBehavior.Restrict` | No accidental data destruction from a parent delete |
| Role names never appear as strings — always `AppRoles.Admin`, `AppRoles.Teacher`, `AppRoles.Student` | A typo in a role string fails silently at runtime, not at compile time |

---

## What I Learned

Three things this project changed about how I think:

**Evaluate tools by what they cost, not just what they provide.** AutoMapper solved a real problem. It also created one. I did not see that trade-off clearly at the start. I only understood it after working with it across enough modules that the debugging cost became obvious. Removing it felt like going backwards, but the codebase was easier to reason about the moment it was gone.

**Architecture decisions compound.** The schedule module went through the most redesigns. The string-based design flaw only became visible when conflict detection became a requirement and could not be patched around. This was also where I understood what working with AI actually means — it proposes, you catch what is wrong, and you fix it. Getting the entity model right before writing service logic is not caution — it is just cheaper in the long run.

**Documenting what broke is worth the time.** Documenting what broke is worth the time. There were bugs I wanted to move past quickly. Writing down the root cause and what changed forced me to actually understand them rather than just fix the symptom.

---

## The Story

I am a PharmD student building toward health tech. I restarted with a clear goal in December 2024: build something production-grade, document everything, and understand every decision before making it. I worked with AI throughout this project — not as a code generator, but as a collaborator I directed, questioned, and frequently corrected. The schedule module alone went through multiple redesigns because I kept identifying flaws in what was being proposed. The mass assignment vulnerability, the false-passing unit tests, the AutoMapper trade-off — none of these were handed to me.

I also sought feedback from senior engineers, implemented their suggestions, and documented what changed and why. That habit shaped the final architecture in real ways.

This repository is also a message to anyone who is afraid to start. The path was not clean. It is documented here exactly as it happened.

---

## Getting Started

**Prerequisites:**
- .NET 8 SDK
- SQL Server (SQLEXPRESS or full)
- Visual Studio 2022 or VS Code

**Setup:**

1. Clone the repository
```bash
git clone https://github.com/hajrahmuzammal-glitch/HHMCoreLMS.git
cd HHMCoreLMS
```

2. Create `appsettings.Development.json` in `HHMCore.WebAPI/`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=HHMCore_LMS_Dev;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "JWT": {
    "Key": "YourSuperSecretKeyHereMustBe32CharactersLong!",
    "Issuer": "HHMCore.API",
    "Audience": "HHMCore.Client",
    "DurationInMinutes": 60
  }
}
```

3. Run migrations
```
Add-Migration InitialCreate -Project HHMCore.Data -StartupProject HHMCore.WebAPI
Update-Database -Project HHMCore.Data -StartupProject HHMCore.WebAPI
```

4. Run the API
```bash
dotnet run --project HHMCore.WebAPI
```

5. Open Swagger at `https://localhost:{port}/swagger`

Default seeded admin credentials are in `DbSeeder.cs` — `admin@hhmcore.com` / `Admin@123456`

---

## Entity Overview

| Entity | Purpose |
|---|---|
| `BaseEntity` | Base for all tables — Id, soft delete, audit fields |
| `AppUser` | Extended Identity user — FullName |
| `Department` | Groups courses, students, teachers |
| `Designation` | Teacher job titles (Lecturer, Professor, HOD) |
| `Building` | Physical buildings on campus |
| `Room` | Rooms within buildings — type, capacity |
| `Course` | A subject with credit hours and semester number |
| `Semester` | Time period (Fall 2025) — only one active at a time |
| `Student` | Student profile linked to Identity user |
| `Teacher` | Teacher profile linked to Identity user |
| `TimeSlot` | Reusable day/time combinations (Mon/Wed 09:00–11:00) |
| `CourseAssignment` | Teacher + Course + Room + TimeSlot + Semester |
| `Enrollment` | Student registered in a course for a semester |
| `Attendance` | Single attendance record per student per class |
| `Assignment` | Homework created by teacher |
| `AssignmentSubmission` | Student submitted answer |
| `Quiz` | Quiz created by teacher |
| `QuizResult` | Student score on a quiz |

---

## What's Next

- Schedule module conflict detection finalization
- Attendance module (single and bulk mark)
- Grading module (assignments and quizzes)
- Admin dashboard with reports
- Angular 21 frontend integration — module by module as each backend ships
- Docker containerization
- Cloud deployment (Azure or Railway)
- `IUnitOfWork` refactor to generic `Repository<T>()` pattern
- Explicit transaction management (`BeginTransactionAsync`, `CommitAsync`, `RollbackAsync`)
- `IMemoryCache` for frequently read static data (departments, rooms, time slots)

---

## Feedback Welcome

If you are working through a similar project and hit an error documented here, open an issue. If something in the architecture could be stronger, open an issue. If this helped you start something you were afraid to start, I would genuinely like to know.

If you found this repository useful, a star helps others find it.

Connect with me on LinkedIn: [www.linkedin.com/in/hafiza-hajrah-muzammal-scripttoscriptbyhhm]

---
