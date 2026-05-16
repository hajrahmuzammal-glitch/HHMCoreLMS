
using System.Runtime.CompilerServices;
using HHMCore.Core.DTOs.Department;
using HHMCore.Core.Entities;

namespace HHMCore.Core.Mappings;

public static class DepartmentMappings
{
    public static DepartmentResponseDto ToResponseDto(this Department department) => new()
    {
        //Department Info
        Id = department.Id,
        Name = department.Name,
        Code = department.Code,
        Description = department.Description,

        //Status Info
        IsActive = department.IsActive,

        //Audit Info
        CreatedAt = department.CreatedAt,
        CreatedBy = department.CreatedBy,
        UpdatedAt = department.UpdatedAt,
        UpdatedBy = department.UpdatedBy

    };
    public static Department ToEntity(
        this CreateDepartmentDto dto,
        string createdBy) => new()
        {
            //Department Info
            Name = dto.Name.Trim(),
            Code = dto.Code.ToUpper().Trim(),
            Description = dto.Description?.Trim(),

            //Audit Info
            CreatedBy = createdBy,
        };
    public static void ApplyUpdate(this Department department,
        UpdateDepartmentDto dto)
    {
        //Department Info
        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
           department.Name = dto.Name.Trim();
        }
        if(!string.IsNullOrWhiteSpace(dto.Code))
        {
            department.Code = dto.Code.ToUpper().Trim();
        }
        if(!string.IsNullOrWhiteSpace(dto.Description))
        {
            department.Description = dto.Description.Trim();
        }

        //Active Status Info
        if(dto.IsActive.HasValue)
        {
            department.IsActive = dto.IsActive.Value;
        }

    }
}
