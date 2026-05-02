using AutoMapper;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Course;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;

namespace HHMCore.Core.Services;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CourseService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<CourseResponseDto>> CreateAsync(
        CreateCourseDto dto, string createdBy)
    {
        var normalizedCode = dto.Code.Trim().ToUpper();

        var codeExists = await _unitOfWork.Courses.ExistsAsync(
            c => c.Code.ToUpper() == normalizedCode);
        if (codeExists)
        {
            return ApiResponse<CourseResponseDto>.Fail(
                "A course with this code already exists.");
        }

        var dept = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId);
        if (dept == null)
        {
            return ApiResponse<CourseResponseDto>.Fail("Department not found.");
        }

        var course = new Course
        {
            Name = dto.Name.Trim(),
            Code = normalizedCode,
            Description = dto.Description?.Trim(),
            DepartmentId = dto.DepartmentId,
            CreditHours = dto.CreditHours,
            SemesterNumber = dto.SemesterNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _unitOfWork.Courses.AddAsync(course);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Courses
            .GetByIdWithIncludesAsync(course.Id, c => c.Department);

        var mapped = _mapper.Map<CourseResponseDto>(created);
        return ApiResponse<CourseResponseDto>.Ok(mapped, "Course created successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<CourseResponseDto>>> GetAllAsync()
    {
        var courses = await _unitOfWork.Courses
            .GetAllWithIncludesAsync(c => c.Department);

        var mapped = _mapper.Map<IReadOnlyList<CourseResponseDto>>(courses);
        return ApiResponse<IReadOnlyList<CourseResponseDto>>.Ok(mapped, "Courses fetched.");
    }

    public async Task<ApiResponse<CourseResponseDto>> GetByIdAsync(Guid id)
    {
        var course = await _unitOfWork.Courses
            .GetByIdWithIncludesAsync(id, c => c.Department);

        if (course == null)
        {
            return ApiResponse<CourseResponseDto>.Fail("Course not found.");
        }

        var mapped = _mapper.Map<CourseResponseDto>(course);
        return ApiResponse<CourseResponseDto>.Ok(mapped, "Course fetched.");
    }

    public async Task<ApiResponse<IReadOnlyList<CourseResponseDto>>> GetByDepartmentAsync(
        Guid departmentId)
    {
        var dept = await _unitOfWork.Departments.GetByIdAsync(departmentId);
        if (dept == null)
        {
            return ApiResponse<IReadOnlyList<CourseResponseDto>>.Fail(
                "Department not found.");
        }

        var courses = await _unitOfWork.Courses.FindWithIncludesAsync(
            c => c.DepartmentId == departmentId,
            c => c.Department);

        var mapped = _mapper.Map<IReadOnlyList<CourseResponseDto>>(courses);
        return ApiResponse<IReadOnlyList<CourseResponseDto>>.Ok(mapped, "Courses fetched.");
    }

    public async Task<ApiResponse<CourseResponseDto>> UpdateAsync(
        Guid id, UpdateCourseDto dto, string updatedBy)
    {
        var course = await _unitOfWork.Courses
            .GetByIdWithIncludesAsync(id, c => c.Department);

        if (course == null)
        {
            return ApiResponse<CourseResponseDto>.Fail("Course not found.");
        }

        // Code uniqueness — only check if a new code was actually sent
        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            var normalizedCode = dto.Code.Trim().ToUpper();
            var codeExists = await _unitOfWork.Courses.ExistsAsync(
                c => c.Code.ToUpper() == normalizedCode && c.Id != id);
            if (codeExists)
            {
                return ApiResponse<CourseResponseDto>.Fail(
                    "A course with this code already exists.");
            }
        }

        // Department existence — only check if department is being changed
        if (dto.DepartmentId.HasValue)
        {
            var dept = await _unitOfWork.Departments
                .GetByIdAsync(dto.DepartmentId.Value);
            if (dept == null)
            {
                return ApiResponse<CourseResponseDto>.Fail("Department not found.");
            }
        }

        // Strings — IsNullOrWhiteSpace catches null, empty, and whitespace
        course.Name = string.IsNullOrWhiteSpace(dto.Name)
            ? course.Name : dto.Name.Trim();

        course.Code = string.IsNullOrWhiteSpace(dto.Code)
            ? course.Code : dto.Code.Trim().ToUpper();

        course.Description = string.IsNullOrWhiteSpace(dto.Description)
            ? course.Description : dto.Description.Trim();

        // Value types — ?? keeps existing value if null (not sent)
        course.DepartmentId = dto.DepartmentId ?? course.DepartmentId;
        course.CreditHours = dto.CreditHours ?? course.CreditHours;
        course.SemesterNumber = dto.SemesterNumber ?? course.SemesterNumber;
        course.IsActive = dto.IsActive ?? course.IsActive;

        course.UpdatedAt = DateTime.UtcNow;
        course.UpdatedBy = updatedBy;

        _unitOfWork.Courses.Update(course);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Courses
            .GetByIdWithIncludesAsync(id, c => c.Department);

        var mapped = _mapper.Map<CourseResponseDto>(updated!);
        return ApiResponse<CourseResponseDto>.Ok(mapped, "Course updated successfully.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, string deletedBy)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(id);
        if (course == null)
        {
            return ApiResponse.Fail("Course not found.");
        }

        var hasAssignments = await _unitOfWork.CourseAssignments
            .ExistsAsync(ca => ca.CourseId == id);
        if (hasAssignments)
        {
            return ApiResponse.Fail(
                "Cannot delete a course that has timetable assignments. Remove assignments first.");
        }

        course.UpdatedAt = DateTime.UtcNow;
        course.UpdatedBy = deletedBy;

        _unitOfWork.Courses.Delete(course);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Course deleted successfully.");
    }
}