using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHMCore.Core.Entities
{
    public class Course : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CreditHours { get; set; }
        public int SemesterNumber { get; set; }
        public bool IsActive { get; set; } = true;

        public Guid DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public ICollection<CourseAssignment> CourseAssignments { get; set; } = new List<CourseAssignment>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    }
}
