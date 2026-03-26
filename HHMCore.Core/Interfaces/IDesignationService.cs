using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Designation;

namespace HHMCore.Core.Interfaces;

public interface IDesignationService
{
    Task<ApiResponse<DesignationResponseDto>> CreateAsync(CreateDesignationDto dto, string createdBy);
    Task<ApiResponse<IReadOnlyList<DesignationResponseDto>>> GetAllAsync();
    Task<ApiResponse<DesignationResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<DesignationResponseDto>> UpdateAsync(UpdateDesignationDto dto, string updatedBy);
    Task<ApiResponse> DeleteAsync(Guid id);
}