using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Semester;

namespace HHMCore.Core.Interfaces;

public interface ISemesterService
{
    Task<ApiResponse<SemesterResponseDto>> CreateAsync(CreateSemesterDto dto, string createdBy);
    Task<ApiResponse<IReadOnlyList<SemesterResponseDto>>> GetAllAsync();
    Task<ApiResponse<SemesterResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IReadOnlyList<SemesterResponseDto>>> GetActiveAsync();
    Task<ApiResponse<SemesterResponseDto>> UpdateAsync(UpdateSemesterDto dto, string updatedBy);
    Task<ApiResponse<SemesterResponseDto>> ActivateAsync(Guid id, string updatedBy);
    Task<ApiResponse<SemesterResponseDto>> DeactivateAsync(Guid id, string updatedBy);
    Task<ApiResponse> DeleteAsync(Guid id);
}