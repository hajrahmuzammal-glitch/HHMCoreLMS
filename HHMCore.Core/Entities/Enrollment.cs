using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHMCore.Core.Entities
{
    public class Enrollment : BaseEntity
    {
        public Guid StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public Guid CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public Guid SemesterId { get; set; }
        public Semester Semester { get; set; } = null!;

        public string Status { get; set; } = "Active";
        public string? LetterGrade { get; set; }
        public decimal? GradePoints { get; set; }
        public decimal? MarksObtained { get; set; }
        public decimal? TotalMarks { get; set; }

    }
}
