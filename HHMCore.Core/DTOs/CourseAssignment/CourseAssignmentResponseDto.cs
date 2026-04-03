
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
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public Guid TimeSlotId { get; set; }
    public string TimeSlotLabel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}