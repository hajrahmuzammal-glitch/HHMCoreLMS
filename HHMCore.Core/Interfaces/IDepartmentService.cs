using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Department;

namespace HHMCore.Core.Interfaces;

public interface IDepartmentService
{
    Task<ApiResponse<DepartmentResponseDto>> CreateAsync(CreateDepartmentDto dto, string createdBy);
    Task<ApiResponse<DepartmentResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IReadOnlyList<DepartmentResponseDto>>> GetAllAsync();
    Task<ApiResponse<DepartmentResponseDto>> UpdateAsync(UpdateDepartmentDto dto, string updatedBy);
    Task<ApiResponse> DeleteAsync(Guid id);
}