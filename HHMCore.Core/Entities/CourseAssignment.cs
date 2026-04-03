using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHMCore.Core.Entities
{
    public class CourseAssignment : BaseEntity
    {
        public Guid CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public Guid TeacherId { get; set; }
        public Teacher Teacher { get; set; } = null!;

        public Guid SemesterId { get; set; }
        public Semester Semester { get; set; } = null!;

        public Guid RoomId { get; set; }
        public Room Room { get; set; } = null!;

        public Guid TimeSlotId { get; set; }
        public TimeSlot TimeSlot { get; set; } = null!;

        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    }
}
