using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Student;

namespace HHMCore.Core.Interfaces
{
    public interface IStudentService
    {
        Task<ApiResponse<StudentResponseDto>> CreateAsync(CreateStudentDto dto, string createdBy);
        Task<ApiResponse<StudentResponseDto>> GetByIdAsync(Guid id);
        Task<ApiResponse<IReadOnlyList<StudentResponseDto>>> GetAllAsync();
        Task<ApiResponse<StudentResponseDto>> UpdateAsync(UpdateStudentDto dto, string updatedBy);
        Task<ApiResponse> DeleteAsync(Guid id);
    }
}