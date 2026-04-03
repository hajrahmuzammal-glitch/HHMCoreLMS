using HHMCore.Core.Enums;

namespace HHMCore.Core.Entities;

public class TimeSlot : BaseEntity
{
    public LmsDaysOfWeek Days { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Label { get; set; } = string.Empty;

    public ICollection<CourseAssignment> CourseAssignments { get; set; }
        = new List<CourseAssignment>();
}