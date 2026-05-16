namespace HHMCore.Core.DTOs.Room;

public class RoomResponseDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public Guid BuildingId { get; set; }
    public string BuildingName { get; set; } = string.Empty;
    public string? BuildingCode { get; set; }
    public int Capacity { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    //then we need to see who has deeted these filed too? althoug i am using soft delete
    //and i have the deletedBy in the parameter of the delete function but it should be shown to admin somewhere?
}
