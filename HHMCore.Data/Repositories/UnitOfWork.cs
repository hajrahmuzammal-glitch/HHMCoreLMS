using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using HHMCore.Data.Context;

namespace HHMCore.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        // Each repository is lazy-loaded — only created when first accessed
        private IGenericRepository<Student>? _students;
        private IGenericRepository<Teacher>? _teachers;
        private IGenericRepository<Department>? _departments;
        private IGenericRepository<Course>? _courses;
        private IGenericRepository<Semester>? _semesters;
        private IGenericRepository<CourseAssignment>? _courseAssignments;
        private IGenericRepository<Room>? _rooms;
        private IGenericRepository<TimeSlot>? _timeSlots;
        private IGenericRepository<Enrollment>? _enrollments;
        private IGenericRepository<Attendance>? _attendances;
        private IGenericRepository<Assignment>? _assignments;
        private IGenericRepository<AssignmentSubmission>? _assignmentSubmissions;
        private IGenericRepository<Quiz>? _quizzes;
        private IGenericRepository<QuizResult>? _quizResults;
        private IGenericRepository<FeeRecord>? _feeRecords;
        private IGenericRepository<Designation>? _designations;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        //Lazy Initialization of repositories
        public IGenericRepository<Student> Students =>
            _students ??= new GenericRepository<Student>(_context);

        public IGenericRepository<Teacher> Teachers =>
            _teachers ??= new GenericRepository<Teacher>(_context);

        public IGenericRepository<Department> Departments =>
            _departments ??= new GenericRepository<Department>(_context);

        public IGenericRepository<Course> Courses =>
            _courses ??= new GenericRepository<Course>(_context);

        public IGenericRepository<Semester> Semesters =>
            _semesters ??= new GenericRepository<Semester>(_context);

        public IGenericRepository<CourseAssignment> CourseAssignments =>
            _courseAssignments ??= new GenericRepository<CourseAssignment>(_context);
        public IGenericRepository<Room> Rooms =>
        _rooms ??= new GenericRepository<Room>(_context);

        public IGenericRepository<TimeSlot> TimeSlots =>
            _timeSlots ??= new GenericRepository<TimeSlot>(_context);
        
        public IGenericRepository<Enrollment> Enrollments =>
            _enrollments ??= new GenericRepository<Enrollment>(_context);

        public IGenericRepository<Attendance> Attendances =>
            _attendances ??= new GenericRepository<Attendance>(_context);

        public IGenericRepository<Assignment> Assignments =>
            _assignments ??= new GenericRepository<Assignment>(_context);

        public IGenericRepository<AssignmentSubmission> AssignmentSubmissions =>
            _assignmentSubmissions ??= new GenericRepository<AssignmentSubmission>(_context);

        public IGenericRepository<Quiz> Quizzes =>
            _quizzes ??= new GenericRepository<Quiz>(_context);

        public IGenericRepository<QuizResult> QuizResults =>
            _quizResults ??= new GenericRepository<QuizResult>(_context);

        public IGenericRepository<FeeRecord> FeeRecords =>
            _feeRecords ??= new GenericRepository<FeeRecord>(_context);

        public IGenericRepository<Designation> Designations =>
          _designations ??= new GenericRepository<Designation>(_context);

        // One call saves ALL pending changes across ALL repositories
        public async Task<int> SaveChangesAsync()
             => await _context.SaveChangesAsync();


        // Frees the DbContext from memory when the request is done
        public void Dispose()
            => _context.Dispose();
    }
}
