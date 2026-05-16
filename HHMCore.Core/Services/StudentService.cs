
using AutoMapper;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Student;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace HHMCore.Core.Services;
public class StudentService : IStudentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;

    public StudentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        UserManager<AppUser> userManager,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<ApiResponse<StudentResponseDto>> CreateAsync(
        CreateStudentDto dto, string createdBy)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
        {
            return ApiResponse<StudentResponseDto>.Fail("A user with this email already exists.");
        }

        var rollExists = await _unitOfWork.Students
            .ExistsAsync(s => s.RollNumber == dto.RollNumber.Trim().ToUpper());
        if (rollExists)
        {
            return ApiResponse<StudentResponseDto>.Fail("A student with this roll number already exists.");
        }

        var department = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId);
        if (department is null)
        {
            return ApiResponse<StudentResponseDto>.Fail("Department not found.");
        }

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
            return ApiResponse<StudentResponseDto>.Fail("Failed to create account.", errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(appUser, AppRoles.Student);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(appUser);
            return ApiResponse<StudentResponseDto>.Fail(
                "The 'Student' role does not exist. An Admin must create it first via POST /api/roles.");
        }

        try
        {
            var student = new Student
            {
                UserId = appUser.Id,
                RollNumber = dto.RollNumber.Trim().ToUpper(),
                DepartmentId = dto.DepartmentId,
                CurrentSemesterNumber = dto.CurrentSemesterNumber,
                PhoneNumber = dto.PhoneNumber.Trim(),
                Address = dto.Address.Trim(),
                DateOfBirth = dto.DateOfBirth,
                Status = StudentStatus.Active,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.SaveChangesAsync();

            // Fire-and-forget welcome email — do not block or fail enrollment on email errors
            _ = _emailService.SendCredentialsAsync(
                appUser.Email!,
                appUser.FullName,
                appUser.Email!,
                dto.Password,
                AppRoles.Student);

            var created = await _unitOfWork.Students
                .GetByIdWithIncludesAsync(student.Id, s => s.User, s => s.Department);

            var response = _mapper.Map<StudentResponseDto>(created);
            return ApiResponse<StudentResponseDto>.Ok(response, "Student created successfully.");
        }
        catch (Exception)
        {
            await _userManager.DeleteAsync(appUser);
            return ApiResponse<StudentResponseDto>.Fail("Failed to create student profile. Please try again.");
        }
    }

    public async Task<ApiResponse<StudentResponseDto>> GetByIdAsync(Guid id)
    {
        var student = await _unitOfWork.Students
            .GetByIdWithIncludesAsync(id, s => s.User, s => s.Department);

        if (student is null)
        {
            return ApiResponse<StudentResponseDto>.Fail("Student not found.");
        }

        var response = _mapper.Map<StudentResponseDto>(student);
        return ApiResponse<StudentResponseDto>.Ok(response, "Student retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<StudentResponseDto>>> GetAllAsync()
    {
        var students = await _unitOfWork.Students
            .GetAllWithIncludesAsync(s => s.User, s => s.Department);

        var response = _mapper.Map<IReadOnlyList<StudentResponseDto>>(students);
        return ApiResponse<IReadOnlyList<StudentResponseDto>>.Ok(response, "Students retrieved successfully.");
    }

    public async Task<ApiResponse<StudentResponseDto>> UpdateAsync(
        Guid id, UpdateStudentDto dto, string updatedBy)
    {
        var student = await _unitOfWork.Students
            .GetByIdWithIncludesAsync(id, s => s.User, s => s.Department);

        if (student is null)
        {
            return ApiResponse<StudentResponseDto>.Fail("Student not found.");
        }

        if (!string.IsNullOrWhiteSpace(dto.FullName))
        {
            var appUser = await _userManager.FindByIdAsync(student.UserId);
            if (appUser is not null)
            {
                appUser.FullName = dto.FullName.Trim();
                await _userManager.UpdateAsync(appUser);
            }
        }

        if (dto.DepartmentId.HasValue)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId.Value);
            if (department is null)
            {
                return ApiResponse<StudentResponseDto>.Fail("Department not found.");
            }

            student.DepartmentId = dto.DepartmentId.Value;
        }

        student.Address = string.IsNullOrWhiteSpace(dto.Address) ? student.Address : dto.Address.Trim();
        student.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? student.PhoneNumber : dto.PhoneNumber.Trim();
        student.CurrentSemesterNumber = dto.CurrentSemesterNumber ?? student.CurrentSemesterNumber;
        student.DateOfBirth = dto.DateOfBirth ?? student.DateOfBirth;

        // ← was missing before
        student.UpdatedBy = updatedBy;
        student.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Students.Update(student);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Students
            .GetByIdWithIncludesAsync(id, s => s.User, s => s.Department);

        var response = _mapper.Map<StudentResponseDto>(updated!);
        return ApiResponse<StudentResponseDto>.Ok(response, "Student updated successfully.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, string deletedBy)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(id);
        if (student is null)
        {
            return ApiResponse.Fail("Student not found.");
        }

        student.UpdatedAt = DateTime.UtcNow;
        student.UpdatedBy = deletedBy;

        _unitOfWork.Students.Delete(student);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Student deleted successfully.");
    }

    public async Task<ApiResponse<StudentResponseDto>> GetMeAsync(string userId)
    {
        var student = await _unitOfWork.Students
            .FindOneWithIncludesAsync(s => s.UserId == userId, s => s.User, s => s.Department);

        if (student is null)
        {
            return ApiResponse<StudentResponseDto>.Fail("Student profile not found.");
        }

        var response = _mapper.Map<StudentResponseDto>(student);
        return ApiResponse<StudentResponseDto>.Ok(response, "Profile retrieved successfully.");
    }
}
