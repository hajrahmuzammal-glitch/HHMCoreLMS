using HHMCore.Core.Enums;

namespace HHMCore.Core.DTOs.Room;

public class UpdateRoomDto
{
    public Guid Id { get; set; }
    public string? RoomNumber { get; set; }
    public string? Building { get; set; }
    public int? Capacity { get; set; }
    public RoomType? RoomType { get; set; }
    public bool? IsActive { get; set; }
}