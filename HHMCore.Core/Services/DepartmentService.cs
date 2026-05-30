using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Department;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using HHMCore.Core.Mappings;

namespace HHMCore.Core.Services;

/// <summary>
/// Handles all business logic for department management.
/// </summary>
/// <remarks>
/// Departments are the top-level organizational unit — teachers, students,
/// and courses all belong to a department.
/// </remarks>

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // -------------------------------------------------------------------------
    // CREATE
    // -------------------------------------------------------------------------

    public async Task<ApiResponse<DepartmentResponseDto>> CreateAsync(
        CreateDepartmentDto dto, string createdBy)
    {

        // Name uses case-insensitive comparison to prevent near-duplicates like "CS" and "cs".
        // Code is normalized to uppercase so storage is consistent regardless of what the caller sends.
        var nameExists = await _unitOfWork.Departments
            .ExistsAsync(x => x.Name.ToLower() == dto.Name.ToLower());
        if (nameExists)
            return ApiResponse<DepartmentResponseDto>.Fail(
                "A department with this name already exists.");

        var codeExists = await _unitOfWork.Departments
            .ExistsAsync(x => x.Code == dto.Code.ToUpper());
        if (codeExists)
            return ApiResponse<DepartmentResponseDto>.Fail(
                "A department with this code already exists.");

        var department = dto.ToEntity(createdBy);

        await _unitOfWork.Departments.AddAsync(department);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<DepartmentResponseDto>.Ok(
            department.ToResponseDto(), "Department created successfully.");
    }

    // -------------------------------------------------------------------------
    // READ
    // -------------------------------------------------------------------------

    public async Task<ApiResponse<DepartmentResponseDto>> GetByIdAsync(Guid id)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id);
        if (department == null)
            return ApiResponse<DepartmentResponseDto>.Fail("Department not found.");

        return ApiResponse<DepartmentResponseDto>.Ok(department.ToResponseDto());
    }

    public async Task<ApiResponse<IReadOnlyList<DepartmentResponseDto>>> GetAllAsync()
    {
        var departments = await _unitOfWork.Departments.GetAllAsync();

        var response = departments
            .Select(d => d.ToResponseDto())
            .ToList();

        return ApiResponse<IReadOnlyList<DepartmentResponseDto>>.Ok(response);
    }

    // -------------------------------------------------------------------------
    // UPDATE
    // -------------------------------------------------------------------------

    public async Task<ApiResponse<DepartmentResponseDto>> UpdateAsync(
        Guid id, UpdateDepartmentDto dto, string updatedBy)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id);
        if (department == null)
            return ApiResponse<DepartmentResponseDto>.Fail("Department not found.");

        // Duplicate checks only run when the field is actually being changed.
        // The current record is excluded from the check using d.Id != id.
        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var nameExists = await _unitOfWork.Departments
                .ExistsAsync(d => d.Name.ToLower() == dto.Name.ToLower() && d.Id != id);
            if (nameExists)
                return ApiResponse<DepartmentResponseDto>.Fail(
                    "A department with this name already exists.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            var codeExists = await _unitOfWork.Departments
                .ExistsAsync(d => d.Code.ToUpper() == dto.Code.ToUpper() && d.Id != id);
            if (codeExists)
                return ApiResponse<DepartmentResponseDto>.Fail(
                    "A department with this code already exists.");
        }

        // ApplyUpdate handles null-safe field merging — only provided fields are overwritten.
        department.ApplyUpdate(dto);
        department.UpdatedAt = DateTimeOffset.UtcNow;
        department.UpdatedBy = updatedBy;

        _unitOfWork.Departments.Update(department);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<DepartmentResponseDto>.Ok(
            department.ToResponseDto(), "Department updated successfully.");
    }

    // -------------------------------------------------------------------------
    // DELETE
    // -------------------------------------------------------------------------

    public async Task<ApiResponse> DeleteAsync(Guid id, string deletedBy)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(id);
        if (department == null)
            return ApiResponse.Fail("Department not found.");

        // Referential integrity is enforced at the service layer.
        // A department cannot be deleted while any dependent records exist,
        // regardless of their active/inactive state.
        var hasTeachers = await _unitOfWork.Teachers
            .ExistsAsync(t => t.DepartmentId == id);
        if (hasTeachers)
            return ApiResponse.Fail(
                "Cannot delete this department. Teachers are assigned to it.");

        var hasStudents = await _unitOfWork.Students
            .ExistsAsync(s => s.DepartmentId == id);
        if (hasStudents)
            return ApiResponse.Fail(
                "Cannot delete this department. Students are enrolled in it.");

        var hasCourses = await _unitOfWork.Courses
            .ExistsAsync(c => c.DepartmentId == id);
        if (hasCourses)
            return ApiResponse.Fail(
                "Cannot delete this department. Courses are assigned to it.");

        // CourseAssignment carries a denormalized DepartmentId for conflict detection.
        // This is checked independently because soft-deleting a course does not
        // cascade to its assignments — orphaned assignments would remain.
        var hasCourseAssignments = await _unitOfWork.CourseAssignments
            .ExistsAsync(ca => ca.DepartmentId == id);
        if (hasCourseAssignments)
            return ApiResponse.Fail(
                "Cannot delete this department. Course assignments are linked to it.");

        //Soft delete — audit fields record who removed this and when.
        // Stamp audit fields before delete — entity state is unreliable after.

        department.UpdatedAt = DateTimeOffset.UtcNow;
        department.UpdatedBy = deletedBy;

        _unitOfWork.Departments.Delete(department);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Department deleted successfully.");
    }
}
