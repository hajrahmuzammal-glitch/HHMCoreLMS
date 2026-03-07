using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHMCore.Core.Entities
{
    public class Assignment : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalMarks { get; set; }
        public string? FilePath { get; set; }
        public bool IsActive { get; set; } = true;

        public Guid CourseAssignmentId { get; set; }
        public CourseAssignment CourseAssignment { get; set; } = null!;

        public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();

    }
}
