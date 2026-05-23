
using HHMCore.Core.DTOs.Teacher;
using HHMCore.Core.Entities;

namespace HHMCore.Core.Mappings;

public static class TeacherMappings
{
    public static TeacherResponseDto ToResponseDto(this Teacher teacher) => new()
    {
        Id = teacher.Id,
        FullName = teacher.FullName,
        Email = teacher.User.Email!,
        EmployeeId = teacher.EmployeeId,
        Cnic = teacher.Cnic,
        DesignationId = teacher.DesignationId,
        DesignationTitle = teacher.Designation.Title,
        Gender = teacher.Gender.ToString(),
        Salary = teacher.Salary,
        PhoneNumber = teacher.PhoneNumber,
        Address = teacher.Address,
        DateOfBirth = teacher.DateOfBirth,
        DepartmentId = teacher.DepartmentId,
        DepartmentName = teacher.Department.Name,
        Qualification = teacher.Qualification,
        JoiningDate = teacher.JoiningDate,
        Status = teacher.Status,
        CreatedBy = teacher.CreatedBy,
        CreatedAt = teacher.CreatedAt,
        UpdatedAt = teacher.UpdatedAt,
        UpdatedBy = teacher.UpdatedBy

    };


    public static Teacher ToEntity(
        this CreateTeacherDto dto,
        string userId,
        string employeeId,
        string createdBy) => new()
        {
            //Employee Info
            UserId = userId,
            FullName = dto.FullName.Trim(), 
            EmployeeId = employeeId,
            Cnic = dto.Cnic.Trim(),
            DateOfBirth = dto.DateOfBirth,
            PhoneNumber = dto.PhoneNumber.Trim(),
            Address = dto.Address.Trim(),
            Gender = dto.Gender,
            Qualification = dto.Qualification.Trim(),

            //Joining Info
            JoiningDate = dto.JoiningDate,
            Salary = dto.Salary,

            //Navigation Properties
            DesignationId = dto.DesignationId,
            DepartmentId = dto.DepartmentId,

            //Audit Info
            CreatedBy = createdBy,
        };


    public static void ApplyUpdate(this Teacher teacher, UpdateTeacherDto dto)
    {
        //Personal Info

        if (!string.IsNullOrWhiteSpace(dto.FullName))
        {
            teacher.FullName = dto.FullName.Trim();
        }
        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            teacher.PhoneNumber = dto.PhoneNumber.Trim();
        }
        if (!string.IsNullOrWhiteSpace(dto.Address))
        {
            teacher.Address = dto.Address.Trim();
        }
        if (!string.IsNullOrWhiteSpace(dto.Qualification))
        {
            teacher.Qualification = dto.Qualification.Trim();
        }

        //------- Enum values ----------//

        if (dto.Gender.HasValue)
        {
            teacher.Gender = dto.Gender.Value;
        }

        //------- Date Time Values ----------//
        if (dto.DateOfBirth.HasValue)
        {
            teacher.DateOfBirth = dto.DateOfBirth.Value;
        }
        if (dto.JoiningDate.HasValue)
        {
            teacher.JoiningDate = dto.JoiningDate.Value;
        }

        //------- Numeric Values ----------//

        if (dto.Salary.HasValue)
        {
            teacher.Salary = dto.Salary.Value;
        }

        //------- FK Ids values ----------//
        if (dto.DesignationId.HasValue)
        {
            teacher.DesignationId = dto.DesignationId.Value;
        }
        if (dto.DepartmentId.HasValue)
        {
            teacher.DepartmentId = dto.DepartmentId.Value;
        }

    }
    public static void ApplyProfileUpdate(this Teacher teacher, UpdateTeacherProfileDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            teacher.PhoneNumber = dto.PhoneNumber.Trim();
        }
        if (!string.IsNullOrWhiteSpace(dto.Address))
        {
            teacher.Address = dto.Address.Trim();
        }
        if (!string.IsNullOrWhiteSpace(dto.Qualification))
        {
            teacher.Qualification = dto.Qualification.Trim();
        }
    }
}
