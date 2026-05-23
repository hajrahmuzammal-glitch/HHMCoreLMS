namespace HHMCore.Core.Services;


using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Teacher;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using HHMCore.Core.Mappings;
using Microsoft.AspNetCore.Identity;

public class TeacherService : ITeacherService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;

    public TeacherService(
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<ApiResponse<TeacherResponseDto>> CreateAsync(
        CreateTeacherDto dto, string createdBy)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
        {
            return ApiResponse<TeacherResponseDto>.Fail("A user with this email already exists.");
        }

        var cnicExists = await _unitOfWork.Teachers.ExistsAsync(t => t.Cnic == dto.Cnic);
        if (cnicExists)
        {
            return ApiResponse<TeacherResponseDto>.Fail("A teacher with this CNIC already exists.");
        }

        var department = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId);
        if (department is null)
        {
            return ApiResponse<TeacherResponseDto>.Fail("Department not found.");
        }

        var designation = await _unitOfWork.Designations.GetByIdAsync(dto.DesignationId);
        if (designation is null)
        {
            return ApiResponse<TeacherResponseDto>.Fail("Designation not found.");
        }


        var employeeId = await GenerateEmployeeIdAsync();

        var appUser = new AppUser
        {
            FullName = dto.FullName.Trim(),
            Email = normalizedEmail,
            UserName = normalizedEmail
        };

        var userResult = await _userManager.CreateAsync(appUser, dto.Password);
        if (!userResult.Succeeded)
        {
            var errors = userResult.Errors.Select(e => e.Description).ToList();
            return ApiResponse<TeacherResponseDto>.Fail("Failed to create account.", errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(appUser, AppRoles.Teacher);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(appUser);
            return ApiResponse<TeacherResponseDto>.Fail(
                "The 'Teacher' role does not exist. An Admin must create it first via POST /api/roles.");
        }

        try
        {
            var teacher = dto.ToEntity(appUser.Id, employeeId, createdBy);

            await _unitOfWork.Teachers.AddAsync(teacher);
            await _unitOfWork.SaveChangesAsync();

            // Fire-and-forget — email failure must never roll back a successful creation
            _ = _emailService.SendCredentialsAsync(
                appUser.Email!,
                appUser.FullName,
                appUser.Email!,
                dto.Password,
                AppRoles.Teacher);

            var created = await _unitOfWork.Teachers
                .GetByIdWithIncludesAsync(teacher.Id, t => t.User, t => t.Department, t => t.Designation);

            if (created is null)
            {
                return ApiResponse<TeacherResponseDto>.Fail("Teacher created but could not be retrieved.");
            }

            var response = created.ToResponseDto();
            return ApiResponse<TeacherResponseDto>.Ok(response, "Teacher created successfully.");
        }
        catch
        {
            await _userManager.DeleteAsync(appUser);
            return ApiResponse<TeacherResponseDto>.Fail("An error occurred while creating the teacher.");
        }
    }

    public async Task<ApiResponse<IReadOnlyList<TeacherResponseDto>>> GetAllAsync()
    {
        var teachers = await _unitOfWork.Teachers
            .GetAllWithIncludesAsync(t => t.User, t => t.Department, t => t.Designation);

        var response = teachers.Select(t => t.ToResponseDto()).ToList();
        return ApiResponse<IReadOnlyList<TeacherResponseDto>>.Ok(response, "Teachers retrieved successfully.");
    }

    public async Task<ApiResponse<TeacherResponseDto>> GetByIdAsync(Guid id)
    {
        var teacher = await _unitOfWork.Teachers
            .GetByIdWithIncludesAsync(id, t => t.User, t => t.Department, t => t.Designation);

        if (teacher is null)
        {
            return ApiResponse<TeacherResponseDto>.Fail("Teacher not found.");
        }

        var response = teacher.ToResponseDto();
        return ApiResponse<TeacherResponseDto>.Ok(response, "Teacher retrieved successfully.");
    }

    public async Task<ApiResponse<TeacherResponseDto>> GetMeAsync(string userId)
    {
        var teacher = await _unitOfWork.Teachers
            .FindOneWithIncludesAsync(
                t => t.UserId == userId,
                t => t.User, t => t.Department, t => t.Designation);

        if (teacher is null)
        {
            return ApiResponse<TeacherResponseDto>.Fail("Teacher profile not found.");
        }

        var response = teacher.ToResponseDto();
        return ApiResponse<TeacherResponseDto>.Ok(response, "Profile retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<TeacherResponseDto>>> GetByDepartmentAsync(Guid departmentId)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
        if (department is null)
        {
            return ApiResponse<IReadOnlyList<TeacherResponseDto>>.Fail("Department not found.");
        }

        var teachers = await _unitOfWork.Teachers
            .FindWithIncludesAsync(
                t => t.DepartmentId == departmentId,
                t => t.User, t => t.Department, t => t.Designation);

        var response = teachers.Select(t => t.ToResponseDto()).ToList();
        return ApiResponse<IReadOnlyList<TeacherResponseDto>>.Ok(response, "Teachers retrieved successfully.");
    }

    public async Task<ApiResponse<TeacherResponseDto>> UpdateMyProfileAsync(
        string userId, UpdateTeacherProfileDto dto)
    {
        var teacher = await _unitOfWork.Teachers
            .FindOneWithIncludesAsync(
                t => t.UserId == userId,
                t => t.User, t => t.Department, t => t.Designation);

        if (teacher is null)
        {
            return ApiResponse<TeacherResponseDto>.Fail("Teacher profile not found.");
        }

        teacher.ApplyProfileUpdate(dto);

        teacher.UpdatedAt = DateTimeOffset.UtcNow;
        teacher.UpdatedBy = userId;

        _unitOfWork.Teachers.Update(teacher);
        await _unitOfWork.SaveChangesAsync();

        var response = teacher.ToResponseDto();
        return ApiResponse<TeacherResponseDto>.Ok(response, "Profile updated successfully.");
    }

    public async Task<ApiResponse<TeacherResponseDto>> UpdateAsync(
       Guid id, UpdateTeacherDto dto, string updatedBy)
    {
        var teacher = await _unitOfWork.Teachers
            .GetByIdWithIncludesAsync(id, t => t.User, t => t.Department, t => t.Designation);

        if (teacher is null)
        {
            return ApiResponse<TeacherResponseDto>.Fail("Teacher not found.");
        }

        if (dto.DesignationId.HasValue)
        {
            var designation = await _unitOfWork.Designations.GetByIdAsync(dto.DesignationId.Value);
            if (designation is null)
            {
                return ApiResponse<TeacherResponseDto>.Fail("Designation not found.");
            }
        }

        if (dto.DepartmentId.HasValue)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId.Value);
            if (department is null)
            {
                return ApiResponse<TeacherResponseDto>.Fail("Department not found");
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.FullName))
        {
            var appUser = await _userManager.FindByIdAsync(teacher.UserId);
            if (appUser is not null)
            {
                appUser.FullName = dto.FullName.Trim();
                await _userManager.UpdateAsync(appUser);
            }
        }

        teacher.ApplyUpdate(dto);
        teacher.UpdatedBy = updatedBy;
        teacher.UpdatedAt = DateTimeOffset.UtcNow;

        _unitOfWork.Teachers.Update(teacher);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Teachers
            .GetByIdWithIncludesAsync(teacher.Id, t => t.User, t => t.Department, t => t.Designation);

        if (updated is null)
        {
            return ApiResponse<TeacherResponseDto>.Fail("Teacher updated but could not be retrieved.");
        }

        var response = updated.ToResponseDto();
        return ApiResponse<TeacherResponseDto>.Ok(response, "Teacher updated successfully.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, string deletedBy)
    {
        var teacher = await _unitOfWork.Teachers.GetByIdAsync(id);
        if (teacher is null)
        {
            return ApiResponse.Fail("Teacher not found.");
        }

        teacher.UpdatedAt = DateTimeOffset.UtcNow;
        teacher.UpdatedBy = deletedBy;

        _unitOfWork.Teachers.Delete(teacher);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Teacher deleted successfully.");
    }

    // Private Helpers //

    //Employee Id Generator

    private async Task<string> GenerateEmployeeIdAsync()
    {
        var allTeachers = await _unitOfWork.Teachers.GetAllAsync();
        var currentYear = DateTimeOffset.UtcNow.Year.ToString();

        var lastNumber = allTeachers
            .Where(t => t.EmployeeId.StartsWith($"EMP-{currentYear}-"))
            .Select(t =>
            {
                var parts = t.EmployeeId.Split('-');
                return parts.Length == 3 && int.TryParse(parts[2], out var n) ? n : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        return $"EMP-{currentYear}-{(lastNumber + 1):D4}";
    }
}
