namespace HHMCore.Core.DTOs.Room;

public class RoomResponseDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}