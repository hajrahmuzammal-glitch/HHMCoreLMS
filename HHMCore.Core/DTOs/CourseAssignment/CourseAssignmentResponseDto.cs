// HHMCore.Core/DTOs/CourseAssignment/CourseAssignmentResponseDto.cs

namespace HHMCore.Core.DTOs.CourseAssignment;

public class CourseAssignmentResponseDto
{
    public Guid Id { get; set; }

    // Course
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public int CreditHours { get; set; }

    // Department
    public string DepartmentName { get; set; } = string.Empty;

    // Teacher
    public string TeacherName { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;

    // Room
    public string RoomNumber { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public int RoomCapacity { get; set; }

    // TimeSlot
    public string TimeSlotLabel { get; set; } = string.Empty;
    public string Days { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;

    // Semester
    public string SemesterName { get; set; } = string.Empty;

    // Own
    public string Section { get; set; } = string.Empty;
    public int MaxEnrollment { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
