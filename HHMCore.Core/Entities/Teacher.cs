using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHMCore.Core.Entities
{
    public class Teacher : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public string FullName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string Cnic { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string Designation { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public DateTime JoiningDate { get; set; } = DateTime.UtcNow;
        public decimal Salary { get; set; }
        public string Status { get; set; } = "Active";
        public string? ProfileImagePath { get; set; }

        public Guid DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public ICollection<CourseAssignment> CourseAssignments { get; set; } = new List<CourseAssignment>();

    }
}
