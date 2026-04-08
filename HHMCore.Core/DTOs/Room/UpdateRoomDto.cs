using HHMCore.Core.Enums;

namespace HHMCore.Core.DTOs.Room;

public class UpdateRoomDto
{
    public string? RoomNumber { get; set; }
    public Guid? BuildingId { get; set; }
    public int? Capacity { get; set; }
    public RoomType? RoomType { get; set; }
    public bool? IsActive { get; set; }
}