
using AutoMapper;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Department;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;

namespace HHMCore.Core.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<DepartmentResponseDto>> CreateAsync(CreateDepartmentDto dto, string createdBy)
    {
        var codeExists = await _unitOfWork.Departments
           .ExistsAsync(x => x.Code == dto.Code.ToUpper());

        if (codeExists)
        {
            return ApiResponse<DepartmentResponseDto>.Fail("A department with this code already exists.");
        }

        var department = new Department
        {
            Name = dto.Name,
            Code = dto.Code.ToUpper(),
            Description = dto.Description,
            IsActive = true,
            //CreatedAt = DateTimeOffset.UtcNow, we dont need to intiliaze it here as we already haev done in the base entity
            //will think about the cons too
            CreatedBy = createdBy
        };

        await _unitOfWork.Departments.AddAsync(department);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<DepartmentResponseDto>(department);
        return ApiResponse<DepartmentResponseDto>.Ok(response, "Department created successfully.");
    }

    public async Task<ApiResponse<DepartmentResponseDto>> GetByIdAsync(Guid id)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id);

        if (department == null)
        {
            return ApiResponse<DepartmentResponseDto>.Fail("Department not found.");
        }

        var response = _mapper.Map<DepartmentResponseDto>(department);
        return ApiResponse<DepartmentResponseDto>.Ok(response);
    }

    public async Task<ApiResponse<IReadOnlyList<DepartmentResponseDto>>> GetAllAsync()
    {
        var departments = await _unitOfWork.Departments.GetAllAsync();
        var response = _mapper.Map<IReadOnlyList<DepartmentResponseDto>>(departments);
        return ApiResponse<IReadOnlyList<DepartmentResponseDto>>.Ok(response);
    }


    public async Task<ApiResponse<DepartmentResponseDto>> UpdateAsync(
        Guid id,
        UpdateDepartmentDto dto,
        string updatedBy)
    {
        // Step 1 — Find the existing record. If it's not there, fail immediately.
        var department = await _unitOfWork.Departments.GetByIdAsync(id);
        if (department == null)
        {
            return ApiResponse<DepartmentResponseDto>.Fail("Department not found.");
        }

        // Step 2 — Apply only the fields that were actually sent.
        // IsNullOrWhiteSpace catches null, "", and "   " — all three mean "don't change it."
        // ?? is used for value types (bool, int, Guid) which cannot be an empty string.

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            // Check for duplicate name (excluding this record itself)
            var nameExists = await _unitOfWork.Departments.ExistsAsync(
                d => d.Name.ToLower() == dto.Name.ToLower() && d.Id != id);
            if (nameExists)
            {
                return ApiResponse<DepartmentResponseDto>.Fail(
                    "A department with this name already exists.");
            }

            department.Name = dto.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            var codeExists = await _unitOfWork.Departments.ExistsAsync(
                d => d.Code.ToUpper() == dto.Code.ToUpper() && d.Id != id);
            if (codeExists)
            {
                return ApiResponse<DepartmentResponseDto>.Fail(
                    "A department with this code already exists.");
            }

            department.Code = dto.Code.ToUpper().Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Description))
        {
            department.Description = dto.Description.Trim();
        }

        // bool? — use ?? because bool cannot be empty string
        department.IsActive = dto.IsActive ?? department.IsActive;

        // Step 3 — Stamp who updated it and when
        department.UpdatedAt = DateTimeOffset.UtcNow;
        department.UpdatedBy = updatedBy;

        // Step 4 — Tell EF Core this record changed, then save
        _unitOfWork.Departments.Update(department);
        await _unitOfWork.SaveChangesAsync();

        // Step 5 — Map to response DTO and return
        var responseDto = _mapper.Map<DepartmentResponseDto>(department);
        return ApiResponse<DepartmentResponseDto>.Ok(responseDto, "Department updated successfully.");
    }
    public async Task<ApiResponse> DeleteAsync(Guid id, string deletedBy)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id);

        if (department == null)
        {
            return ApiResponse.Fail("Department not found.");
        }
        var hasTeachers = await _unitOfWork.Teachers

        .ExistsAsync(t => t.DepartmentId == id);
        if (hasTeachers)
        {
            return ApiResponse.Fail("Cannot delete this department. Teachers are assigned to it.");

        }
        var hasStudents = await _unitOfWork.Students
            .ExistsAsync(s => s.DepartmentId == id);
        if (hasStudents)
        {
            return ApiResponse.Fail("Cannot delete this department. Students are enrolled in it.");
        }

        _unitOfWork.Departments.Delete(department);
        department.UpdatedAt = DateTime.UtcNow;
        department.UpdatedBy = deletedBy;

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Department deleted successfully.");
    }
}
