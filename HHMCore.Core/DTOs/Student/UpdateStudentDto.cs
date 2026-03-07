using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHMCore.Core.DTOs.Student
{
    public class UpdateStudentDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int CurrentSemesterNumber { get; set; }
        public Guid DepartmentId { get; set; }
    }
}