using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Teacher;

namespace HHMCore.Core.Interfaces;

public interface ITeacherService
{
    Task<ApiResponse<TeacherResponseDto>> CreateAsync(CreateTeacherDto dto, string createdBy);
    Task<ApiResponse<IReadOnlyList<TeacherResponseDto>>> GetAllAsync();
    Task<ApiResponse<TeacherResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<TeacherResponseDto>> GetMeAsync(string userId);
    Task<ApiResponse<IReadOnlyList<TeacherResponseDto>>> GetByDepartmentAsync(Guid departmentId);
    Task<ApiResponse<TeacherResponseDto>> UpdateAsync(Guid id, UpdateTeacherDto dto, string updatedBy);
    Task<ApiResponse<TeacherResponseDto>> UpdateMyProfileAsync(string userId, UpdateTeacherProfileDto dto);
    Task<ApiResponse> DeleteAsync(Guid id, string deletedBy);

}
