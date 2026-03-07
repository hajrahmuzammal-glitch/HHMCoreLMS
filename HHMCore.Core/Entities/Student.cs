using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHMCore.Core.Entities
{
    public class Student : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public string FullName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Active";
        public int CurrentSemesterNumber { get; set; } = 1;
        public string? ProfileImagePath { get; set; }

        public Guid DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<FeeRecord> FeeRecords { get; set; } = new List<FeeRecord>();
        public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
        public ICollection<QuizResult> QuizResults { get; set; } = new List<QuizResult>();

    }
}
