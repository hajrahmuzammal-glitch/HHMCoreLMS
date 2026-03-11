using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHMCore.Core.DTOs.Teacher
{
    public class UpdateTeacherDto
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; } 
        public string? Designation { get; set; }
        public decimal? Salary { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}