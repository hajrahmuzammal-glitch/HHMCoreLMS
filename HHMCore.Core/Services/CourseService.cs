using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Course;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    public async Task<ApiResponse<CourseResponseDto>> CreateAsync(CreateCourseDto dto, string createdBy)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId);
        if (department == null)
            return ApiResponse<CourseResponseDto>.Fail("Department not found.");

        var existing = await _unitOfWork.Courses.FindAsync(x => x.Code == dto.Code);
        if (existing.Any())
            return ApiResponse<CourseResponseDto>.Fail("A course with this code already exists.");

        var course = new Course
        {
            Name = dto.Name,
            Code = dto.Code.ToUpper(),
            Description = dto.Description,
            CreditHours = dto.CreditHours,
            SemesterNumber = dto.SemesterNumber,
            DepartmentId = dto.DepartmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _unitOfWork.Courses.AddAsync(course);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Courses.GetByIdWithIncludesAsync(
            course.Id,
            c => c.Department
        );

        var response = _mapper.Map<CourseResponseDto>(created);
        return ApiResponse<CourseResponseDto>.Ok(response, "Course created successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<CourseResponseDto>>> GetAllAsync()
    {
        var courses = await _unitOfWork.Courses.GetAllWithIncludesAsync(
            c => c.Department
        );

        var response = _mapper.Map<IReadOnlyList<CourseResponseDto>>(courses);
        return ApiResponse<IReadOnlyList<CourseResponseDto>>.Ok(response, "Courses retrieved successfully.");
    }

    public async Task<ApiResponse<CourseResponseDto>> GetByIdAsync(Guid id)
    {
        var course = await _unitOfWork.Courses.GetByIdWithIncludesAsync(
            id,
            c => c.Department
        );

        if (course == null)
            return ApiResponse<CourseResponseDto>.Fail("Course not found.");

        var response = _mapper.Map<CourseResponseDto>(course);
        return ApiResponse<CourseResponseDto>.Ok(response, "Course retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<CourseResponseDto>>> GetByDepartmentAsync(Guid departmentId)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
        if (department == null)
            return ApiResponse<IReadOnlyList<CourseResponseDto>>.Fail("Department not found.");

        var courses = await _unitOfWork.Courses.FindWithIncludesAsync(
            x => x.DepartmentId == departmentId,
            c => c.Department
        );

        var response = _mapper.Map<IReadOnlyList<CourseResponseDto>>(courses);
        return ApiResponse<IReadOnlyList<CourseResponseDto>>.Ok(response, "Courses retrieved successfully.");
    }

    public async Task<ApiResponse<CourseResponseDto>> UpdateAsync(Guid id, UpdateCourseDto dto, string updatedBy)
    {
        if (id != dto.Id)
            return ApiResponse<CourseResponseDto>.Fail("ID in URL does not match ID in body.");

        var course = await _unitOfWork.Courses.GetByIdAsync(id);
        if (course == null)
            return ApiResponse<CourseResponseDto>.Fail("Course not found.");

        var department = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId);
        if (department == null)
            return ApiResponse<CourseResponseDto>.Fail("Department not found.");

        var duplicate = await _unitOfWork.Courses.FindAsync(
            x => x.Code == dto.Code.ToUpper() && x.Id != id
        );
        if (duplicate.Any())
            return ApiResponse<CourseResponseDto>.Fail("A course with this code already exists.");

        course.Name = dto.Name;
        course.Code = dto.Code.ToUpper();
        course.Description = dto.Description;
        course.CreditHours = dto.CreditHours;
        course.SemesterNumber = dto.SemesterNumber;
        course.DepartmentId = dto.DepartmentId;
        course.IsActive = dto.IsActive;
        course.UpdatedAt = DateTime.UtcNow;
        course.UpdatedBy = updatedBy;

        _unitOfWork.Courses.Update(course);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Courses.GetByIdWithIncludesAsync(
            course.Id,
            c => c.Department
        );

        var response = _mapper.Map<CourseResponseDto>(updated);
        return ApiResponse<CourseResponseDto>.Ok(response, "Course updated successfully.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(id);
        if (course == null)
            return ApiResponse.Fail("Course not found.");

        _unitOfWork.Courses.Delete(course);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Course deleted successfully.");
    }
}
