
using FluentValidation;
using FluentValidation.AspNetCore;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using HHMCore.Core.Mappings;
using HHMCore.Core.Services;
using HHMCore.Core.Validators.Auth;
using HHMCore.Data.Context;
using HHMCore.Data.Repositories;
using HHMCore.Data.Seeders;
using HHMCore.WebAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------
// 1. SERILOG — logging to console and file
// ----------------------------------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ----------------------------------------
// 2. DATABASE — connect to SQL Server
// ----------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ----------------------------------------
// 3. IDENTITY — user management + roles
// ----------------------------------------
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Password rules
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ----------------------------------------
// 4. JWT AUTHENTICATION
// ----------------------------------------
var jwtKey = builder.Configuration["JWT:Key"]!;
var jwtIssuer = builder.Configuration["JWT:Issuer"]!;
var jwtAudience = builder.Configuration["JWT:Audience"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// ----------------------------------------
// 5. REPOSITORY + UNIT OF WORK
// ----------------------------------------
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ----------------------------------------
// 6. SERVICES — business logic
// ----------------------------------------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IDesignationService, DesignationService>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ICourseAssignmentService, CourseAssignmentService>();

// ----------------------------------------
// 7. AUTOMAPPER
// ----------------------------------------
builder.Services.AddAutoMapper(typeof(AuthMappingProfile));

// ----------------------------------------
// 8. FLUENT VALIDATION
// ----------------------------------------
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();

// ----------------------------------------
// 9. CONTROLLERS
// ----------------------------------------
builder.Services.AddControllers();

// ----------------------------------------
// 10. SWAGGER — API documentation + testing
// ----------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HHMCore LMS API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token only — no 'Bearer' prefix needed"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ----------------------------------------
// BUILD THE APP
// ----------------------------------------
var app = builder.Build();

// ----------------------------------------
// RUN DBSEEDER — create roles + admin user
// ----------------------------------------
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await DbSeeder.SeedAsync(userManager, roleManager);
}

// ----------------------------------------
// MIDDLEWARE PIPELINE — ORDER MATTERS
// ----------------------------------------
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();