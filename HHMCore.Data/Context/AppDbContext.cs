
using HHMCore.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HHMCore.Data.Context
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<CourseAssignment> CourseAssignments { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizResult> QuizResults { get; set; }
        public DbSet<FeeRecord> FeeRecords { get; set; }
        public DbSet<Designation> Designations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Department>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Student>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Teacher>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Course>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Semester>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<CourseAssignment>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Enrollment>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Attendance>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Assignment>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<AssignmentSubmission>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Quiz>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<QuizResult>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<FeeRecord>().HasQueryFilter(x => !x.IsDeleted);
            builder.Entity<Designation>().HasQueryFilter(x => !x.IsDeleted);

            builder.Entity<Teacher>()
                .Property(x => x.Salary)
                .HasColumnType("decimal(18,2)");

            builder.Entity<FeeRecord>()
                .Property(x => x.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<FeeRecord>()
                .Property(x => x.LateFine)
                .HasColumnType("decimal(18,2)");

            builder.Entity<FeeRecord>()
                .Property(x => x.Discount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<CourseAssignment>()
                .HasOne(x => x.Course)
                .WithMany(x => x.CourseAssignments)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CourseAssignment>()
                .HasOne(x => x.Teacher)
                .WithMany(x => x.CourseAssignments)
                .HasForeignKey(x => x.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CourseAssignment>()
                .HasOne(x => x.Semester)
                .WithMany(x => x.CourseAssignments)
                .HasForeignKey(x => x.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Enrollment>()
                .HasOne(x => x.Course)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Enrollment>()
                .HasOne(x => x.Semester)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Enrollment>()
                .Property(e => e.GradePoints)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Enrollment>()
                .Property(e => e.MarksObtained)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Enrollment>()
                .Property(e => e.TotalMarks)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Attendance>()
                .HasOne(x => x.CourseAssignment)
                .WithMany(x => x.Attendances)
                .HasForeignKey(x => x.CourseAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Attendance>()
                .HasOne(x => x.Student)
                .WithMany(x => x.Attendances)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Assignment>()
                .Property(a => a.TotalMarks)
                .HasColumnType("decimal(18,2)");

            builder.Entity<AssignmentSubmission>()
                 .Property(a => a.MarksObtained)
                 .HasColumnType("decimal(18,2)");

            builder.Entity<AssignmentSubmission>()
                .HasOne(x => x.Assignment)
                .WithMany(x => x.Submissions)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Quiz>()
                 .Property(q => q.TotalMarks)
                 .HasColumnType("decimal(18,2)");

       
            builder.Entity<QuizResult>()
                .Property(q => q.MarksObtained)
                .HasColumnType("decimal(18,2)");

            builder.Entity<QuizResult>()
                .HasOne(x => x.Quiz)
                .WithMany(x => x.QuizResults)
                .HasForeignKey(x => x.QuizId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Teacher>()
                .HasOne(x => x.Designation)
                .WithMany(x => x.Teachers)
                .HasForeignKey(x => x.DesignationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Assignment>()
                .HasOne(x => x.CourseAssignment)
                .WithMany(x => x.Assignments)
                .HasForeignKey(x => x.CourseAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AssignmentSubmission>()
                .HasOne(x => x.Student)
                .WithMany(x => x.Submissions)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Course>()
                .HasOne(x => x.Department)
                .WithMany(x => x.Courses)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Enrollment>()
                .HasOne(x => x.Student)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FeeRecord>()
                .HasOne(x => x.Student)
                .WithMany(x => x.FeeRecords)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FeeRecord>()
                .HasOne(x => x.Semester)
                .WithMany(x => x.FeeRecords)
                .HasForeignKey(x => x.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Quiz>()
                .HasOne(x => x.CourseAssignment)
                .WithMany(x => x.Quizzes)
                .HasForeignKey(x => x.CourseAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<QuizResult>()
                .HasOne(x => x.Student)
                .WithMany(x => x.QuizResults)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Student>()
                .HasOne(x => x.Department)
                .WithMany(x => x.Students)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Student>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Teacher>()
                .HasOne(x => x.Department)
                .WithMany(x => x.Teachers)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Teacher>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}