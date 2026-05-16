using HHMCore.Core.Enums;

namespace HHMCore.Core.DTOs.Room;

public class CreateRoomDto
{
    public string RoomNumber { get; set; } = string.Empty;
    public Guid BuildingId { get; set; }
    public int Capacity { get; set; }
    public RoomType RoomType { get; set; }
}