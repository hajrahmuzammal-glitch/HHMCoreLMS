using HHMCore.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HHMCore.Data.Context;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Department> Departments { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Designation> Designations { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Semester> Semesters { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<TimeSlot> TimeSlots { get; set; }
    public DbSet<CourseAssignment> CourseAssignments { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<QuizResult> QuizResults { get; set; }
    public DbSet<FeeRecord> FeeRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── Global Query Filters ──────────────────────────────────────────
        builder.Entity<Department>().HasQueryFilter(d => !d.IsDeleted);
        builder.Entity<Student>().HasQueryFilter(s => !s.IsDeleted);
        builder.Entity<Teacher>().HasQueryFilter(t => !t.IsDeleted);
        builder.Entity<Designation>().HasQueryFilter(d => !d.IsDeleted);
        builder.Entity<Course>().HasQueryFilter(c => !c.IsDeleted);
        builder.Entity<Semester>().HasQueryFilter(s => !s.IsDeleted);
        builder.Entity<Room>().HasQueryFilter(r => !r.IsDeleted);
        builder.Entity<TimeSlot>().HasQueryFilter(ts => !ts.IsDeleted);
        builder.Entity<CourseAssignment>().HasQueryFilter(ca => !ca.IsDeleted);
        builder.Entity<Enrollment>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Attendance>().HasQueryFilter(a => !a.IsDeleted);
        builder.Entity<Assignment>().HasQueryFilter(a => !a.IsDeleted);
        builder.Entity<AssignmentSubmission>().HasQueryFilter(s => !s.IsDeleted);
        builder.Entity<Quiz>().HasQueryFilter(q => !q.IsDeleted);
        builder.Entity<QuizResult>().HasQueryFilter(r => !r.IsDeleted);
        builder.Entity<FeeRecord>().HasQueryFilter(f => !f.IsDeleted);

        // ── Teacher ───────────────────────────────────────────────────────
        builder.Entity<Teacher>()
            .HasOne(t => t.Designation)
            .WithMany(d => d.Teachers)
            .HasForeignKey(t => t.DesignationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Teacher>()
            .HasOne(t => t.Department)
            .WithMany(d => d.Teachers)
            .HasForeignKey(t => t.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Teacher>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Teacher>()
            .Property(t => t.Salary)
            .HasColumnType("decimal(18,2)");

        // ── Student ───────────────────────────────────────────────────────
        builder.Entity<Student>()
            .HasOne(s => s.Department)
            .WithMany(d => d.Students)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Student>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Course ────────────────────────────────────────────────────────
        builder.Entity<Course>()
            .HasOne(c => c.Department)
            .WithMany(d => d.Courses)
            .HasForeignKey(c => c.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── CourseAssignment ──────────────────────────────────────────────
        builder.Entity<CourseAssignment>()
            .HasOne(ca => ca.Teacher)
            .WithMany(t => t.CourseAssignments)
            .HasForeignKey(ca => ca.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CourseAssignment>()
            .HasOne(ca => ca.Course)
            .WithMany(c => c.CourseAssignments)
            .HasForeignKey(ca => ca.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CourseAssignment>()
            .HasOne(ca => ca.Semester)
            .WithMany(s => s.CourseAssignments)
            .HasForeignKey(ca => ca.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CourseAssignment>()
            .HasOne(ca => ca.Room)
            .WithMany(r => r.CourseAssignments)
            .HasForeignKey(ca => ca.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CourseAssignment>()
            .HasOne(ca => ca.TimeSlot)
            .WithMany(ts => ts.CourseAssignments)
            .HasForeignKey(ca => ca.TimeSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Enrollment ────────────────────────────────────────────────────
        builder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Enrollment>()
            .HasOne(e => e.Semester)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Enrollment>()
            .Property(e => e.GradePoints).HasColumnType("decimal(18,2)");

        builder.Entity<Enrollment>()
            .Property(e => e.MarksObtained).HasColumnType("decimal(18,2)");

        builder.Entity<Enrollment>()
            .Property(e => e.TotalMarks).HasColumnType("decimal(18,2)");

        // ── Attendance ────────────────────────────────────────────────────
        builder.Entity<Attendance>()
            .HasOne(a => a.Student)
            .WithMany(s => s.Attendances)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Attendance>()
            .HasOne(a => a.CourseAssignment)
            .WithMany(ca => ca.Attendances)
            .HasForeignKey(a => a.CourseAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Assignment ────────────────────────────────────────────────────
        builder.Entity<Assignment>()
            .HasOne(a => a.CourseAssignment)
            .WithMany(ca => ca.Assignments)
            .HasForeignKey(a => a.CourseAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Assignment>()
            .Property(a => a.TotalMarks).HasColumnType("decimal(18,2)");

        // ── AssignmentSubmission ──────────────────────────────────────────
        builder.Entity<AssignmentSubmission>()
            .HasOne(s => s.Assignment)
            .WithMany(a => a.Submissions)
            .HasForeignKey(s => s.AssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AssignmentSubmission>()
            .HasOne(s => s.Student)
            .WithMany(st => st.Submissions)
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AssignmentSubmission>()
            .Property(s => s.MarksObtained).HasColumnType("decimal(18,2)");

        // ── Quiz ──────────────────────────────────────────────────────────
        builder.Entity<Quiz>()
            .HasOne(q => q.CourseAssignment)
            .WithMany(ca => ca.Quizzes)
            .HasForeignKey(q => q.CourseAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Quiz>()
            .Property(q => q.TotalMarks).HasColumnType("decimal(18,2)");

        // ── QuizResult ────────────────────────────────────────────────────
        builder.Entity<QuizResult>()
            .HasOne(r => r.Quiz)
            .WithMany(q => q.QuizResults)
            .HasForeignKey(r => r.QuizId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QuizResult>()
            .HasOne(r => r.Student)
            .WithMany(s => s.QuizResults)
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<QuizResult>()
            .Property(r => r.MarksObtained).HasColumnType("decimal(18,2)");

        // ── FeeRecord ─────────────────────────────────────────────────────
        builder.Entity<FeeRecord>()
            .HasOne(f => f.Student)
            .WithMany(s => s.FeeRecords)
            .HasForeignKey(f => f.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FeeRecord>()
            .HasOne(f => f.Semester)
            .WithMany(s => s.FeeRecords)
            .HasForeignKey(f => f.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FeeRecord>()
            .Property(f => f.Amount).HasColumnType("decimal(18,2)");

        builder.Entity<FeeRecord>()
            .Property(f => f.LateFine).HasColumnType("decimal(18,2)");

        builder.Entity<FeeRecord>()
            .Property(f => f.Discount).HasColumnType("decimal(18,2)");
    }
}