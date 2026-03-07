// Location: HHMCore.Core/Services/DepartmentService.cs
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
        // Check if department with same code already exists
        var existing = await _unitOfWork.Departments
            .FindAsync(x => x.Code == dto.Code.ToUpper());

        if (existing.Any())
            return ApiResponse<DepartmentResponseDto>.Fail("A department with this code already exists.");

        var department = new Department
        {
            Name = dto.Name,
            Code = dto.Code.ToUpper(),
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
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
            return ApiResponse<DepartmentResponseDto>.Fail("Department not found.");

        var response = _mapper.Map<DepartmentResponseDto>(department);
        return ApiResponse<DepartmentResponseDto>.Ok(response);
    }

    public async Task<ApiResponse<IReadOnlyList<DepartmentResponseDto>>> GetAllAsync()
    {
        var departments = await _unitOfWork.Departments.GetAllAsync();
        var response = _mapper.Map<IReadOnlyList<DepartmentResponseDto>>(departments);
        return ApiResponse<IReadOnlyList<DepartmentResponseDto>>.Ok(response);
    }

    public async Task<ApiResponse<DepartmentResponseDto>> UpdateAsync(UpdateDepartmentDto dto, string updatedBy)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(dto.Id);

        if (department == null)
            return ApiResponse<DepartmentResponseDto>.Fail("Department not found.");

        // Check if another department already uses this code — exclude current one from check
        var existing = await _unitOfWork.Departments
            .FindAsync(x => x.Code == dto.Code.ToUpper() && x.Id != dto.Id);

        if (existing.Any())
            return ApiResponse<DepartmentResponseDto>.Fail("Another department with this code already exists.");

        department.Name = dto.Name;
        department.Code = dto.Code.ToUpper();
        department.Description = dto.Description;
        department.IsActive = dto.IsActive;
        department.UpdatedAt = DateTime.UtcNow;
        department.UpdatedBy = updatedBy;

        _unitOfWork.Departments.Update(department);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<DepartmentResponseDto>(department);
        return ApiResponse<DepartmentResponseDto>.Ok(response, "Department updated successfully.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id);

        if (department == null)
            return ApiResponse.Fail("Department not found.");

        _unitOfWork.Departments.Delete(department);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Department deleted successfully.");
    }
}