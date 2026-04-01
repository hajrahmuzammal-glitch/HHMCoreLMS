
namespace HHMCore.Core.DTOs.CourseAssignment;

public class UpdateCourseAssignmentDto
{
    public Guid Id { get; set; }
    public string? Room { get; set; }
    public string? Schedule { get; set; }
}