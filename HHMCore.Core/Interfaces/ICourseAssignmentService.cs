
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.CourseAssignment;

namespace HHMCore.Core.Interfaces;

public interface ICourseAssignmentService
{
    Task<ApiResponse<CourseAssignmentResponseDto>> CreateAsync(
        CreateCourseAssignmentDto dto, string createdBy);
    Task<ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>> GetAllAsync();
    Task<ApiResponse<CourseAssignmentResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>> GetBySemesterAsync(
        Guid semesterId);
    Task<ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>> GetByTeacherAsync(
        Guid teacherId);
    Task<ApiResponse<CourseAssignmentResponseDto>> UpdateAsync(
        UpdateCourseAssignmentDto dto, string updatedBy);
    Task<ApiResponse> DeleteAsync(Guid id);
}