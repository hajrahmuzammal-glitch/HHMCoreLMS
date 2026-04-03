
namespace HHMCore.Core.DTOs.CourseAssignment;

public class CreateCourseAssignmentDto
{
    public Guid TeacherId { get; set; }
    public Guid CourseId { get; set; }
    public Guid SemesterId { get; set; }
    public Guid RoomId { get; set; }
    public Guid TimeSlotId { get; set; }
}