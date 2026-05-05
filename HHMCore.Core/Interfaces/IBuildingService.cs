using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Building;

namespace HHMCore.Core.Interfaces;

public interface IBuildingService
{
    Task<ApiResponse<BuildingResponseDto>> CreateAsync(CreateBuildingDto dto, string createdBy);
    Task<ApiResponse<IReadOnlyList<BuildingResponseDto>>> GetAllAsync();
    Task<ApiResponse<BuildingResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<BuildingResponseDto>> UpdateAsync(Guid id, UpdateBuildingDto dto, string updatedBy);
    Task<ApiResponse> DeleteAsync(Guid id, string deletedBy);
}
