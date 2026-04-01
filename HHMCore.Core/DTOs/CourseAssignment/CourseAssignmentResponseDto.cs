
namespace HHMCore.Core.DTOs.CourseAssignment;

public class CourseAssignmentResponseDto
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public Guid SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public string? Room { get; set; }
    public string? Schedule { get; set; }
    public DateTime CreatedAt { get; set; }
}