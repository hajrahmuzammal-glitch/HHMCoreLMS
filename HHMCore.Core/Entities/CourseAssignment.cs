using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHMCore.Core.Entities
{
    public class CourseAssignment : BaseEntity
    {
        // ── Foreign Keys ──────────────────────────────────────────────────────────
        public Guid TeacherId { get; set; }
        public Guid CourseId { get; set; }
        public Guid SemesterId { get; set; }
        public Guid RoomId { get; set; }
        public Guid TimeSlotId { get; set; }

        // Denormalized for fast conflict detection — avoids JOIN through Course
        public Guid DepartmentId { get; set; }

        // ── Navigation Properties ─────────────────────────────────────────────────
        public Teacher Teacher { get; set; } = null!;
        public Course Course { get; set; } = null!;
        public Semester Semester { get; set; } = null!;
        public Room Room { get; set; } = null!;
        public TimeSlot TimeSlot { get; set; } = null!;
        public Department Department { get; set; } = null!;

        // ── Own Properties ────────────────────────────────────────────────────────
        public string Section { get; set; } = string.Empty;
        public int MaxEnrollment { get; set; }

        // ── Child Collections ─────────────────────────────────────────────────────
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    }
}
