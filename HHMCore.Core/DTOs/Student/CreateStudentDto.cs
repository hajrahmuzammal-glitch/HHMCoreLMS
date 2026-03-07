using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHMCore.Core.DTOs.Student
{
    public class CreateStudentDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public int CurrentSemesterNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}