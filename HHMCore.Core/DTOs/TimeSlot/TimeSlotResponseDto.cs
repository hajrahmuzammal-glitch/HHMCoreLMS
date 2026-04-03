namespace HHMCore.Core.DTOs.TimeSlot;

public class TimeSlotResponseDto
{
    public Guid Id { get; set; }
    public int DaysValue { get; set; }
    public string DaysDisplay { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}