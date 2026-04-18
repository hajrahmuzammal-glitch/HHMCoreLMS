
namespace HHMCore.Core.DTOs.CourseAssignment;

public class UpdateCourseAssignmentDto
{
    public Guid? TeacherId { get; set; }
    public Guid? SemesterId { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? TimeSlotId { get; set; }
    public int? MaxEnrollment { get; set; }
    public string? Section { get; set; } //not added byt the ai
}