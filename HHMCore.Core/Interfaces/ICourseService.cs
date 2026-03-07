using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Course;

namespace HHMCore.Core.Interfaces;

public interface ICourseService
{
    Task<ApiResponse<CourseResponseDto>> CreateAsync(CreateCourseDto dto, string createdBy);
    Task<ApiResponse<IReadOnlyList<CourseResponseDto>>> GetAllAsync();
    Task<ApiResponse<CourseResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IReadOnlyList<CourseResponseDto>>> GetByDepartmentAsync(Guid departmentId);
    Task<ApiResponse<CourseResponseDto>> UpdateAsync(Guid id, UpdateCourseDto dto, string updatedBy);
    Task<ApiResponse> DeleteAsync(Guid id);
}
