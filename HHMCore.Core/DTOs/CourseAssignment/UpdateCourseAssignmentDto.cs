
namespace HHMCore.Core.DTOs.CourseAssignment;

public class UpdateCourseAssignmentDto
{
    public Guid Id { get; set; }
    public Guid? TeacherId { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? SemesterId { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? TimeSlotId { get; set; }
}