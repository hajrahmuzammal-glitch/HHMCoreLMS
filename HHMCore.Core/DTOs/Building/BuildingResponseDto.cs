namespace HHMCore.Core.DTOs.Building;

public class BuildingResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int RoomCount { get; set; }   
    public DateTime CreatedAt { get; set; }
}