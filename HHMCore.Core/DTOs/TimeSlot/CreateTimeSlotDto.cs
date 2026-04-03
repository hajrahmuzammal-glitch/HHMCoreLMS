using HHMCore.Core.Enums;

namespace HHMCore.Core.DTOs.TimeSlot;

public class CreateTimeSlotDto
{
    public LmsDaysOfWeek Days { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}