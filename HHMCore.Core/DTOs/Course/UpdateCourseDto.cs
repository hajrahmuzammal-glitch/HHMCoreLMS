using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHMCore.Core.DTOs.Course;

public class UpdateCourseDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; } 
    public string? Code { get; set; } 
    public string? Description { get; set; }
    public int? CreditHours { get; set; }
    public int? SemesterNumber { get; set; }
    public bool? IsActive { get; set; } //told to add ? bcs will make it false by default 
    public Guid? DepartmentId { get; set; }
}