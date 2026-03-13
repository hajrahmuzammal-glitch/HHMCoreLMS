using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HHMCore.Core.DTOs.Designation;

namespace HHMCore.Core.Interfaces;

public interface IDesignationService
{
    Task<DesignationResponseDto> CreateAsync(CreateDesignationDto dto, string createdBy);
    Task<List<DesignationResponseDto>> GetAllAsync();
    Task<DesignationResponseDto> GetByIdAsync(Guid id);
    Task<DesignationResponseDto> UpdateAsync(Guid id, UpdateDesignationDto dto, string updatedBy);
    Task DeleteAsync(Guid id, string deletedBy);
}