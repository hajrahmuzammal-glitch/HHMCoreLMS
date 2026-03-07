# HHMCore LMS — University Learning Management System

A full-stack, production-grade Learning Management System built for 
universities and large educational institutions.

## Tech Stack

**Backend**
- ASP.NET Core 8 Web API
- Entity Framework Core 8 + SQL Server
- ASP.NET Core Identity + JWT Authentication
- Repository Pattern + Unit of Work
- AutoMapper + FluentValidation + Serilog

**Frontend**
- Angular 20 (Standalone Components)
- PrimeNG 17 + PrimeFlex + PrimeIcons

**Architecture**
- Layered Architecture (3-project solution)
- Role-Based Authorization (Admin, Teacher, Student)
- Soft Delete + Global Query Filters
- Generic Repository + Unit of Work Pattern
- Global Exception Middleware
- Standardized API Response Wrapper

## Modules

| Module | Status |
|--------|--------|
| Authentication (Register, Login, JWT) | ✅ Complete |
| Department Management | ✅ Complete |
| Course Management | ✅ Complete |
| Student Module | 🔄 In Progress |
| Teacher Module | ⏳ Planned |
| Attendance Module | ⏳ Planned |
| Grading (Assignments + Quizzes) | ⏳ Planned |
| Fee Management | ⏳ Planned |
| Admin Dashboard + Reports | ⏳ Planned |
| Angular Frontend Integration | ⏳ Planned |

## Architecture
```
HHMCoreLMS/
├── HHMCore.WebAPI    → Controllers, Middleware, Program.cs
├── HHMCore.Core      → Entities, DTOs, Interfaces, Services, Validators
└── HHMCore.Data      → AppDbContext, Repositories, Migrations, Seeders
```

## Key Features

- JWT Bearer Authentication with role-based access control
- Soft delete on all entities — data is never permanently removed
- Standardized API responses across all endpoints
- FluentValidation on all input DTOs
- Global exception handling middleware
- AutoMapper for clean entity-to-DTO mapping
- Database seeding with default roles and admin user

## Getting Started

1. Clone the repository
2. Update connection string in `appsettings.json`
3. Run migrations:
```bash
dotnet ef migrations add InitialCreate -p HHMCore.Data -s HHMCore.WebAPI
dotnet ef database update -p HHMCore.Data -s HHMCore.WebAPI
```
4. Run the project — Swagger UI available at `/swagger`
5. Login with default admin: `admin@hhmcore.com` / `Admin@123456`

## Status
> Actively in development — new modules added weekly.