using HHMCore.Core.Enums;

namespace HHMCore.Core.DTOs.TimeSlot;

public class UpdateTimeSlotDto
{
    public Guid Id { get; set; }
    public LmsDaysOfWeek? Days { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
}