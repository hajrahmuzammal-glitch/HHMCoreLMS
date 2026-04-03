using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Room;

namespace HHMCore.Core.Interfaces;

public interface IRoomService
{
    Task<ApiResponse<RoomResponseDto>> CreateAsync(CreateRoomDto dto, string createdBy);
    Task<ApiResponse<IReadOnlyList<RoomResponseDto>>> CreateBulkAsync(List<CreateRoomDto> dtos, string createdBy);
    Task<ApiResponse<IReadOnlyList<RoomResponseDto>>> GetAllAsync();
    Task<ApiResponse<RoomResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IReadOnlyList<RoomResponseDto>>> GetAvailableRoomsAsync(Guid timeSlotId, Guid semesterId);
    Task<ApiResponse<RoomResponseDto>> UpdateAsync(Guid id, UpdateRoomDto dto, string updatedBy);
    Task<ApiResponse> DeleteAsync(Guid id, string deletedBy);
}