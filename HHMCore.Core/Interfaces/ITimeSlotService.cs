using HHMCore.Core.Common;
using HHMCore.Core.DTOs.TimeSlot;

namespace HHMCore.Core.Interfaces;

public interface ITimeSlotService
{
    Task<ApiResponse<TimeSlotResponseDto>> CreateAsync(CreateTimeSlotDto dto, string createdBy);
    Task<ApiResponse<IReadOnlyList<TimeSlotResponseDto>>> GetAllAsync();
    Task<ApiResponse<TimeSlotResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<TimeSlotResponseDto>> UpdateAsync(Guid id, UpdateTimeSlotDto dto, string updatedBy);
    Task<ApiResponse> DeleteAsync(Guid id, string deletedBy);
}