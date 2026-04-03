using HHMCore.Core.Enums;

namespace HHMCore.Core.DTOs.Room;

public class CreateRoomDto
{
    public string RoomNumber { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public RoomType RoomType { get; set; }
}