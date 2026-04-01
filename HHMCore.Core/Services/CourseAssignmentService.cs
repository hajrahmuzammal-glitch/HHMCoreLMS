
using AutoMapper;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.CourseAssignment;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;

namespace HHMCore.Core.Services;

public class CourseAssignmentService : ICourseAssignmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CourseAssignmentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<CourseAssignmentResponseDto>> CreateAsync(
        CreateCourseAssignmentDto dto, string createdBy)
    {
        var teacherExists = await _unitOfWork.Teachers.ExistsAsync(
            t => t.Id == dto.TeacherId);
        if (!teacherExists)
            return ApiResponse<CourseAssignmentResponseDto>.Fail("Teacher not found.");

        var courseExists = await _unitOfWork.Courses.ExistsAsync(
            c => c.Id == dto.CourseId);
        if (!courseExists)
            return ApiResponse<CourseAssignmentResponseDto>.Fail("Course not found.");

        var semesterExists = await _unitOfWork.Semesters.ExistsAsync(
            s => s.Id == dto.SemesterId);
        if (!semesterExists)
            return ApiResponse<CourseAssignmentResponseDto>.Fail("Semester not found.");

        var duplicate = await _unitOfWork.CourseAssignments.ExistsAsync(
            ca => ca.TeacherId == dto.TeacherId
               && ca.CourseId == dto.CourseId
               && ca.SemesterId == dto.SemesterId);

        if (duplicate)
            return ApiResponse<CourseAssignmentResponseDto>.Fail(
                "This teacher is already assigned to this course in the selected semester.");

        var assignment = new CourseAssignment
        {
            Id = Guid.NewGuid(),
            TeacherId = dto.TeacherId,
            CourseId = dto.CourseId,
            SemesterId = dto.SemesterId,
            Room = dto.Room?.Trim(),
            Schedule = dto.Schedule?.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _unitOfWork.CourseAssignments.AddAsync(assignment);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.CourseAssignments
            .GetByIdWithIncludesAsync(
                assignment.Id,
                ca => ca.Teacher,
                ca => ca.Teacher.User,
                ca => ca.Course,
                ca => ca.Semester);

        var response = _mapper.Map<CourseAssignmentResponseDto>(created);
        return ApiResponse<CourseAssignmentResponseDto>.Ok(
            response, "Course assigned successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>> GetAllAsync()
    {
        var assignments = await _unitOfWork.CourseAssignments
            .GetAllWithIncludesAsync(
                ca => ca.Teacher,
                ca => ca.Teacher.User,
                ca => ca.Course,
                ca => ca.Semester);

        var response = _mapper.Map<IReadOnlyList<CourseAssignmentResponseDto>>(assignments);
        return ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>.Ok(
            response, "Course assignments fetched successfully.");
    }

    public async Task<ApiResponse<CourseAssignmentResponseDto>> GetByIdAsync(Guid id)
    {
        var assignment = await _unitOfWork.CourseAssignments
            .GetByIdWithIncludesAsync(
                id,
                ca => ca.Teacher,
                ca => ca.Teacher.User,
                ca => ca.Course,
                ca => ca.Semester);

        if (assignment == null)
            return ApiResponse<CourseAssignmentResponseDto>.Fail(
                "Course assignment not found.");

        var response = _mapper.Map<CourseAssignmentResponseDto>(assignment);
        return ApiResponse<CourseAssignmentResponseDto>.Ok(
            response, "Course assignment fetched successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>>
        GetBySemesterAsync(Guid semesterId)
    {
        var semesterExists = await _unitOfWork.Semesters.ExistsAsync(
            s => s.Id == semesterId);
        if (!semesterExists)
            return ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>.Fail(
                "Semester not found.");

        var assignments = await _unitOfWork.CourseAssignments
            .FindWithIncludesAsync(
                ca => ca.SemesterId == semesterId,
                ca => ca.Teacher,
                ca => ca.Teacher.User,
                ca => ca.Course,
                ca => ca.Semester);

        var response = _mapper.Map<IReadOnlyList<CourseAssignmentResponseDto>>(assignments);
        return ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>.Ok(
            response, "Course assignments fetched successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>>
        GetByTeacherAsync(Guid teacherId)
    {
        var teacherExists = await _unitOfWork.Teachers.ExistsAsync(
            t => t.Id == teacherId);
        if (!teacherExists)
            return ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>.Fail(
                "Teacher not found.");

        var assignments = await _unitOfWork.CourseAssignments
            .FindWithIncludesAsync(
                ca => ca.TeacherId == teacherId,
                ca => ca.Teacher,
                ca => ca.Teacher.User,
                ca => ca.Course,
                ca => ca.Semester);

        var response = _mapper.Map<IReadOnlyList<CourseAssignmentResponseDto>>(assignments);
        return ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>.Ok(
            response, "Teacher schedule fetched successfully.");
    }

    public async Task<ApiResponse<CourseAssignmentResponseDto>> UpdateAsync(
        UpdateCourseAssignmentDto dto, string updatedBy)
    {
        var assignment = await _unitOfWork.CourseAssignments.GetByIdAsync(dto.Id);
        if (assignment == null)
            return ApiResponse<CourseAssignmentResponseDto>.Fail(
                "Course assignment not found.");

        assignment.Room = string.IsNullOrWhiteSpace(dto.Room)
            ? assignment.Room : dto.Room.Trim();
        assignment.Schedule = string.IsNullOrWhiteSpace(dto.Schedule)
            ? assignment.Schedule : dto.Schedule.Trim();
        assignment.UpdatedAt = DateTime.UtcNow;
        assignment.UpdatedBy = updatedBy;

        _unitOfWork.CourseAssignments.Update(assignment);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.CourseAssignments
            .GetByIdWithIncludesAsync(
                assignment.Id,
                ca => ca.Teacher,
                ca => ca.Teacher.User,
                ca => ca.Course,
                ca => ca.Semester);

        var response = _mapper.Map<CourseAssignmentResponseDto>(updated);
        return ApiResponse<CourseAssignmentResponseDto>.Ok(
            response, "Course assignment updated successfully.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        var assignment = await _unitOfWork.CourseAssignments.GetByIdAsync(id);
        if (assignment == null)
            return ApiResponse.Fail("Course assignment not found.");

        var hasAttendance = await _unitOfWork.Attendances.ExistsAsync(
            a => a.CourseAssignmentId == id);
        if (hasAttendance)
            return ApiResponse.Fail(
                "Cannot delete this assignment. Attendance records exist for it.");

        _unitOfWork.CourseAssignments.Delete(assignment);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Course assignment deleted successfully.");
    }
}