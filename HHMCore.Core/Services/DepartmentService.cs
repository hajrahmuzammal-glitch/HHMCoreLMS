
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Department;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using HHMCore.Core.Mappings;

namespace HHMCore.Core.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    //Create //

    public async Task<ApiResponse<DepartmentResponseDto>> CreateAsync(CreateDepartmentDto dto, string createdBy)
    {
        //Check for duplicate Name and Code //

        //Duplicate Name Check
        var nameExists = await _unitOfWork.Departments
           .ExistsAsync(x => x.Name.ToLower() == dto.Name.ToLower());
        if (nameExists)
        {
            return ApiResponse<DepartmentResponseDto>.Fail("A department with this Name already exists.");
        }

        //Duplicate Code Check
        var codeExists = await _unitOfWork.Departments
            .ExistsAsync(x => x.Code == dto.Code.ToUpper());

        if (codeExists)
        {
            return ApiResponse<DepartmentResponseDto>.Fail("A department with this Code already exists.");
        }

        //Instantiation of Department entity Object

        var department = dto.ToEntity(createdBy);

        //Adding to database
        await _unitOfWork.Departments.AddAsync(department);

        //saving 
        await _unitOfWork.SaveChangesAsync();

        //building response
        var response =department.ToResponseDto();
        return ApiResponse<DepartmentResponseDto>.Ok(response, "Department created successfully.");
    }

    //Get department By Id
    public async Task<ApiResponse<DepartmentResponseDto>> GetByIdAsync(Guid id)
    {
        //Fetching the department 
        var department = await _unitOfWork.Departments.GetByIdAsync(id);

        if (department == null)
        {
            return ApiResponse<DepartmentResponseDto>.Fail("Department not found.");
        }

        //Building Response
        var response = department.ToResponseDto();
        return ApiResponse<DepartmentResponseDto>.Ok(response);
    }

    //Get All Departments
    public async Task<ApiResponse<IReadOnlyList<DepartmentResponseDto>>> GetAllAsync()
    {
        //fetching all departments
        var departments = await _unitOfWork.Departments.GetAllAsync();

        //Building Response
        var response = departments.Select(t => t.ToResponseDto()).ToList();
        return ApiResponse<IReadOnlyList<DepartmentResponseDto>>.Ok(response);
    }

    //Update 
    public async Task<ApiResponse<DepartmentResponseDto>> UpdateAsync(
        Guid id,
        UpdateDepartmentDto dto,
        string updatedBy)
    {
        //Checking if the department exists
        var department = await _unitOfWork.Departments.GetByIdAsync(id);
        if (department == null)
        {
            return ApiResponse<DepartmentResponseDto>.Fail("Department not found.");
        }

        //Updating feilds provided 
        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            // Check for duplicate name 
            var nameExists = await _unitOfWork.Departments.ExistsAsync(
                d => d.Name.ToLower() == dto.Name.ToLower() && d.Id != id);
            if (nameExists)
            {
                return ApiResponse<DepartmentResponseDto>.Fail(
                    "A department with this name already exists.");
            }
        }

        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            // Check for duplicate Code
            var codeExists = await _unitOfWork.Departments.ExistsAsync(
                d => d.Code.ToUpper() == dto.Code.ToUpper() && d.Id != id);
            if (codeExists)
            {
                return ApiResponse<DepartmentResponseDto>.Fail(
                    "A department with this code already exists.");
            }
        }

        //Updating the department entity w
        department.ApplyUpdate(dto);

        //Updating audit fields
        department.UpdatedAt = DateTimeOffset.UtcNow;
        department.UpdatedBy = updatedBy;

        //saving changes
        _unitOfWork.Departments.Update(department);
        await _unitOfWork.SaveChangesAsync();

        //Building Response
        var responseDto = department.ToResponseDto();
        return ApiResponse<DepartmentResponseDto>.Ok(responseDto, "Department updated successfully.");
    }

    //Delete

    public async Task<ApiResponse> DeleteAsync(Guid id, string deletedBy)
    {
        //Checking if the department exists
        var department = await _unitOfWork.Departments.GetByIdAsync(id);

        if (department == null)
        {
            return ApiResponse.Fail("Department not found.");
        }

        //Check for associated teachers and students before deletion//
        //Teacher Dependency Check
        var hasTeachers = await _unitOfWork.Teachers

        .ExistsAsync(t => t.DepartmentId == id);
        if (hasTeachers)
        {
            return ApiResponse.Fail("Cannot delete this department. Teachers are assigned to it.");

        }

        //Student Dependency Check
        var hasStudents = await _unitOfWork.Students
            .ExistsAsync(s => s.DepartmentId == id);
        if (hasStudents)
        {
            return ApiResponse.Fail("Cannot delete this department. Students are enrolled in it.");
        }

        //Soft Deletion - Marking as Inactive
        _unitOfWork.Departments.Delete(department);

        //Updating audit fields
        department.UpdatedAt = DateTimeOffset.UtcNow;
        department.UpdatedBy = deletedBy;

        //saveing changes
        await _unitOfWork.SaveChangesAsync();

        //building response
        return ApiResponse.Ok("Department deleted successfully.");
    }
}
