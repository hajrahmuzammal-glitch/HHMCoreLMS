using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHMCore.Core.Entities
{
    public class Quiz : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public DateTime QuizDate { get; set; }
        public decimal TotalMarks { get; set; }

        public Guid CourseAssignmentId { get; set; }
        public CourseAssignment CourseAssignment { get; set; } = null!;

        public ICollection<QuizResult> QuizResults { get; set; } = new List<QuizResult>();

    }
    public class QuizResult : BaseEntity
    {
        public Guid QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;

        public Guid StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public decimal MarksObtained { get; set; }
        public string? Remarks { get; set; }
    }
}
