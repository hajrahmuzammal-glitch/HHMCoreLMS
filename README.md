# HHMCore LMS

![Build and Test](https://github.com/hajrahmuzammal-glitch/HHMCoreLMS/actions/workflows/build-and-test.yml/badge.svg)

**A University Learning Management System built with ASP.NET Core 8 Web API**

A portfolio-grade, production-architecture LMS designed for universities — covering student management, teacher management, scheduling, attendance, grading, and fee management. Built with clean layered architecture and industry-standard patterns.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8 Web API |
| Database | SQL Server (SQLEXPRESS) |
| ORM | Entity Framework Core 8 |
| Auth | ASP.NET Core Identity + JWT |
| Validation | FluentValidation 11 |
| Mapping | AutoMapper 13 |
| Logging | Serilog (console + file) |
| Frontend | Angular 20 (standalone components) |
| UI Library | PrimeNG 17 + PrimeFlex |

---

## Architecture

Three-project layered architecture with strict separation of concerns.

```
HHMCoreLMS.sln
├── HHMCore.WebAPI    → Controllers, Middleware, Program.cs
├── HHMCore.Core      → Entities, DTOs, Interfaces, Services, Validators, Mappings
└── HHMCore.Data      → AppDbContext, Repositories, Migrations, Seeders
```

**Project dependencies:**
- `HHMCore.WebAPI` → depends on Core + Data
- `HHMCore.Data` → depends on Core only
- `HHMCore.Core` → depends on nothing

This means the database layer can be swapped without touching business logic. Business logic has zero knowledge of HTTP or EF Core internals.

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
| Fee Management | ⬜ Planned | — |
| Admin Dashboard | ⬜ Planned | — |

---

## Key Design Decisions

### Role-Based Access Control
Three roles: **Admin**, **Teacher**, **Student**. Seeded via `DbSeeder` — never hardcoded as strings. All role references use an `AppRoles` constants class.

### Two-DTO Security Pattern
Each role gets only the DTO it needs. A teacher updating their profile gets `UpdateTeacherProfileDto` (phone, address, qualification only). An admin gets `UpdateTeacherDto` (salary, department, designation). This eliminates mass assignment attacks at the contract level — not just with guards.

### Soft Delete Only
No record is ever hard-deleted. `IsDeleted = true` is set on deletion, and a Global Query Filter automatically excludes deleted records from every query. Universities require audit trails.

### Conflict Detection (Schedule Module)
Before saving any course assignment, the system runs four conflict checks:
1. **Room conflict** — same room already booked at that time slot
2. **Teacher conflict** — teacher already assigned to another class at that time
3. **Course conflict** — course already scheduled at that time
4. **Student conflict** — department + semester students already have a class at that time

All four must pass before the schedule entry saves.

### Consistent API Response
Every endpoint returns the same wrapper — success or failure:

```json
{
  "success": true,
  "message": "Teacher created successfully.",
  "data": { },
  "errors": null
}
```

Angular can check `response.success` uniformly without per-endpoint handling.

---

## Patterns Used

**Repository Pattern** — A generic repository wraps all EF Core queries. Services never write raw LINQ against DbContext.

**Unit of Work** — All changes are staged and committed in a single transaction. If any operation fails midway, nothing saves.

**FluentValidation** — All input validation is separate from DTOs. No `[Required]` or `[MaxLength]` attributes anywhere. Validators are composable and testable.

**AutoMapper** — Entity-to-DTO mapping is centralized in profile classes. Services never manually copy fields.

**Global Exception Middleware** — Unhandled exceptions are caught, logged via Serilog, and returned as a consistent `ApiResponse` with no stack trace exposure.

---

## API Highlights

### Auth
```
POST  /api/auth/login       → { email, password }
GET   /api/auth/me          → Bearer token required
```

### Schedule
```
POST  /api/schedules        → Admin only. Enforces all 4 conflict rules.
GET   /api/schedules/semester/{semesterId}  → Full timetable view
GET   /api/rooms/available?timeSlotId=&semesterId=  → Available rooms for slot
```

### Role-separated Teacher endpoints
```
GET   /api/teachers/me              → Teacher sees own profile only
PUT   /api/teachers/me/profile      → Teacher updates own profile (3 fields max)
PUT   /api/teachers/{id}            → Admin only (salary, department, designation)
```

---

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (SQLEXPRESS or full)
- Visual Studio 2022 or VS Code

### Setup

1. Clone the repository
```bash
git clone https://github.com/yourusername/HHMCoreLMS.git
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

Default seeded admin credentials are configured in `DbSeeder.cs`.
admin: `admin@hhmcore.com` / `Admin@123456`
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
| `Attendance` | Single attendance record per student per day |
| `Assignment` | Homework created by teacher |
| `AssignmentSubmission` | Student's submitted answer |
| `Quiz` | Quiz created by teacher |
| `QuizResult` | Student score on a quiz |
| `FeeRecord` | One fee bill per student |

---

## Project Rules (enforced in every PR)

- PUT request body never contains an `Id` field — ID comes from the route only
- After POST/PUT, navigation properties are always re-fetched with `.Include()` before mapping the response — null fields in responses are a bug
- Duplicate/conflict checks happen in the service layer before any insert
- Business logic never appears in controllers
- `decimal` columns always use `HasColumnType("decimal(18,2)")`
- All cascade deletes use `DeleteBehavior.Restrict`
- Role names never appear as strings — always `AppRoles.Admin`, `AppRoles.Teacher`, `AppRoles.Student`

---

## What's Next

- Schedule module conflict detection finalization
- Attendance module (single + bulk mark)
- Grading module (assignments + quizzes)
- Fee management
- Admin dashboard with reports
- Angular 20 frontend integration (module by module as each backend module ships)
- Unit tests with xUnit (service layer)
- Docker containerization
- Cloud deployment (Azure / Railway)

---

## About

Built as a portfolio project demonstrating production-grade ASP.NET Core 8 API development. Designed to be pitched to universities as a real product after completion.

Architecture, patterns, and decisions are documented throughout the development process — including mistakes made, bugs found during Swagger testing, and the reasoning behind each fix.
