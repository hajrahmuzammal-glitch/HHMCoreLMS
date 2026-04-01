
using HHMCore.Core.Entities;


namespace HHMCore.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Student> Students { get; }
        IGenericRepository<Teacher> Teachers { get; }
        IGenericRepository<Department> Departments { get; }
        IGenericRepository<Course> Courses { get; }
        IGenericRepository<Semester> Semesters { get; }
        IGenericRepository<Designation> Designations { get; }
        IGenericRepository<CourseAssignment> CourseAssignments { get; }
        IGenericRepository<Enrollment> Enrollments { get; }
        IGenericRepository<Attendance> Attendances { get; }
        IGenericRepository<Assignment> Assignments { get; }
        IGenericRepository<AssignmentSubmission> AssignmentSubmissions { get; }
        IGenericRepository<Quiz> Quizzes { get; }
        IGenericRepository<QuizResult> QuizResults { get; }
        IGenericRepository<FeeRecord> FeeRecords { get; }

        Task<int> SaveChangesAsync();
    }
}
