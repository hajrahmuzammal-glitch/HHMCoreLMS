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

    private static readonly string[] AssignmentIncludes =
        ["Teacher.User", "Teacher.Department", "Course", "Semester", "Room", "TimeSlot"];

    public CourseAssignmentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<CourseAssignmentResponseDto>> CreateAsync(CreateCourseAssignmentDto dto, string createdBy)
    {
        var teacher = await _unitOfWork.Teachers.GetByIdAsync(dto.TeacherId);
        if (teacher is null)
            return ApiResponse<CourseAssignmentResponseDto>.Fail("Teacher not found.");

        var course = await _unitOfWork.Courses.GetByIdAsync(dto.CourseId);
        if (course is null)
            return ApiResponse<CourseAssignmentResponseDto>.Fail("Course not found.");

        var semester = await _unitOfWork.Semesters.GetByIdAsync(dto.SemesterId);
        if (semester is null)
            return ApiResponse<CourseAssignmentResponseDto>.Fail("Semester not found.");

        var room = await _unitOfWork.Rooms.GetByIdAsync(dto.RoomId);
        if (room is null)
            return ApiResponse<CourseAssignmentResponseDto>.Fail("Room not found.");

        var timeSlot = await _unitOfWork.TimeSlots.GetByIdAsync(dto.TimeSlotId);
        if (timeSlot is null)
            return ApiResponse<CourseAssignmentResponseDto>.Fail("Time slot not found.");

        var roomConflict = await _unitOfWork.CourseAssignments.ExistsAsync(
            ca => ca.RoomId == dto.RoomId &&
                  ca.TimeSlotId == dto.TimeSlotId &&
                  ca.SemesterId == dto.SemesterId);

        if (roomConflict)
            return ApiResponse<CourseAssignmentResponseDto>.Fail(
                $"Room '{room.RoomNumber}' in '{room.Building}' is already booked for this time slot in this semester.");

        var teacherConflict = await _unitOfWork.CourseAssignments.ExistsAsync(
            ca => ca.TeacherId == dto.TeacherId &&
                  ca.TimeSlotId == dto.TimeSlotId &&
                  ca.SemesterId == dto.SemesterId);

        if (teacherConflict)
            return ApiResponse<CourseAssignmentResponseDto>.Fail(
                "This teacher already has a class scheduled at this time slot in this semester.");

        var courseConflict = await _unitOfWork.CourseAssignments.ExistsAsync(
            ca => ca.CourseId == dto.CourseId &&
                  ca.TimeSlotId == dto.TimeSlotId &&
                  ca.SemesterId == dto.SemesterId);

        if (courseConflict)
            return ApiResponse<CourseAssignmentResponseDto>.Fail(
                "This course is already scheduled at this time slot in this semester.");

        var assignment = new CourseAssignment
        {
            Id = Guid.NewGuid(),
            TeacherId = dto.TeacherId,
            CourseId = dto.CourseId,
            SemesterId = dto.SemesterId,
            RoomId = dto.RoomId,
            TimeSlotId = dto.TimeSlotId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _unitOfWork.CourseAssignments.AddAsync(assignment);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.CourseAssignments.GetByIdWithPathIncludesAsync(
            assignment.Id, AssignmentIncludes);

        return ApiResponse<CourseAssignmentResponseDto>.Ok(
            _mapper.Map<CourseAssignmentResponseDto>(created), "Course assignment created successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>> GetAllAsync()
    {
        var assignments = await _unitOfWork.CourseAssignments.GetAllWithPathIncludesAsync(AssignmentIncludes);
        return ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>.Ok(
            _mapper.Map<IReadOnlyList<CourseAssignmentResponseDto>>(assignments), "Assignments fetched.");
    }

    public async Task<ApiResponse<CourseAssignmentResponseDto>> GetByIdAsync(Guid id)
    {
        var assignment = await _unitOfWork.CourseAssignments.GetByIdWithPathIncludesAsync(id, AssignmentIncludes);
        if (assignment is null)
            return ApiResponse<CourseAssignmentResponseDto>.Fail("Assignment not found.");

        return ApiResponse<CourseAssignmentResponseDto>.Ok(
            _mapper.Map<CourseAssignmentResponseDto>(assignment), "Assignment fetched.");
    }

    public async Task<ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>> GetBySemesterAsync(Guid semesterId)
    {
        var assignments = await _unitOfWork.CourseAssignments.FindWithPathIncludesAsync(
            ca => ca.SemesterId == semesterId, AssignmentIncludes);

        return ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>.Ok(
            _mapper.Map<IReadOnlyList<CourseAssignmentResponseDto>>(assignments), "Assignments fetched.");
    }

    public async Task<ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>> GetMyAssignmentsAsync(string userId)
    {
        var teacher = await _unitOfWork.Teachers.FindOneAsync(t => t.UserId == userId);
        if (teacher is null)
            return ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>.Fail("Teacher profile not found.");

        var assignments = await _unitOfWork.CourseAssignments.FindWithPathIncludesAsync(
            ca => ca.TeacherId == teacher.Id, AssignmentIncludes);

        return ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>.Ok(
            _mapper.Map<IReadOnlyList<CourseAssignmentResponseDto>>(assignments), "Your assignments fetched.");
    }
    public async Task<ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>> GetByTeacherAsync(Guid teacherId)
    {
        var teacherExists = await _unitOfWork.Teachers.ExistsAsync(t => t.Id == teacherId);
        if (!teacherExists)
            return ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>.Fail("Teacher not found.");

        var assignments = await _unitOfWork.CourseAssignments
            .FindWithPathIncludesAsync(ca => ca.TeacherId == teacherId, AssignmentIncludes);

        return ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>.Ok(
            _mapper.Map<IReadOnlyList<CourseAssignmentResponseDto>>(assignments),
            "Teacher schedule fetched successfully.");
    }
    public async Task<ApiResponse<CourseAssignmentResponseDto>> UpdateAsync(Guid id, UpdateCourseAssignmentDto dto, string updatedBy)
    {
        var assignment = await _unitOfWork.CourseAssignments.GetByIdAsync(id);
        if (assignment is null)
            return ApiResponse<CourseAssignmentResponseDto>.Fail("Assignment not found.");

        if (dto.TeacherId.HasValue)
        {
            var teacher = await _unitOfWork.Teachers.GetByIdAsync(dto.TeacherId.Value);
            if (teacher is null)
                return ApiResponse<CourseAssignmentResponseDto>.Fail("Teacher not found.");
        }

        if (dto.CourseId.HasValue)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(dto.CourseId.Value);
            if (course is null)
                return ApiResponse<CourseAssignmentResponseDto>.Fail("Course not found.");
        }

        if (dto.SemesterId.HasValue)
        {
            var semester = await _unitOfWork.Semesters.GetByIdAsync(dto.SemesterId.Value);
            if (semester is null)
                return ApiResponse<CourseAssignmentResponseDto>.Fail("Semester not found.");
        }

        if (dto.RoomId.HasValue)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(dto.RoomId.Value);
            if (room is null)
                return ApiResponse<CourseAssignmentResponseDto>.Fail("Room not found.");
        }

        if (dto.TimeSlotId.HasValue)
        {
            var timeSlot = await _unitOfWork.TimeSlots.GetByIdAsync(dto.TimeSlotId.Value);
            if (timeSlot is null)
                return ApiResponse<CourseAssignmentResponseDto>.Fail("Time slot not found.");
        }

        var resolvedRoomId = dto.RoomId ?? assignment.RoomId;
        var resolvedTimeSlotId = dto.TimeSlotId ?? assignment.TimeSlotId;
        var resolvedSemesterId = dto.SemesterId ?? assignment.SemesterId;
        var resolvedTeacherId = dto.TeacherId ?? assignment.TeacherId;
        var resolvedCourseId = dto.CourseId ?? assignment.CourseId;

        var roomConflict = await _unitOfWork.CourseAssignments.ExistsAsync(
            ca => ca.Id != id &&
                  ca.RoomId == resolvedRoomId &&
                  ca.TimeSlotId == resolvedTimeSlotId &&
                  ca.SemesterId == resolvedSemesterId);

        if (roomConflict)
            return ApiResponse<CourseAssignmentResponseDto>.Fail("This room is already booked for this time slot in this semester.");

        var teacherConflict = await _unitOfWork.CourseAssignments.ExistsAsync(
            ca => ca.Id != id &&
                  ca.TeacherId == resolvedTeacherId &&
                  ca.TimeSlotId == resolvedTimeSlotId &&
                  ca.SemesterId == resolvedSemesterId);

        if (teacherConflict)
            return ApiResponse<CourseAssignmentResponseDto>.Fail("This teacher already has a class scheduled at this time slot.");

        var courseConflict = await _unitOfWork.CourseAssignments.ExistsAsync(
            ca => ca.Id != id &&
                  ca.CourseId == resolvedCourseId &&
                  ca.TimeSlotId == resolvedTimeSlotId &&
                  ca.SemesterId == resolvedSemesterId);

        if (courseConflict)
            return ApiResponse<CourseAssignmentResponseDto>.Fail("This course is already scheduled at this time slot.");

        assignment.TeacherId = resolvedTeacherId;
        assignment.CourseId = resolvedCourseId;
        assignment.SemesterId = resolvedSemesterId;
        assignment.RoomId = resolvedRoomId;
        assignment.TimeSlotId = resolvedTimeSlotId;
        assignment.UpdatedAt = DateTime.UtcNow;
        assignment.UpdatedBy = updatedBy;

        _unitOfWork.CourseAssignments.Update(assignment);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.CourseAssignments.GetByIdWithPathIncludesAsync(id, AssignmentIncludes);
        return ApiResponse<CourseAssignmentResponseDto>.Ok(
            _mapper.Map<CourseAssignmentResponseDto>(updated), "Assignment updated successfully.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, string deletedBy)
    {
        var assignment = await _unitOfWork.CourseAssignments.GetByIdAsync(id);
        if (assignment is null)
            return ApiResponse.Fail("Assignment not found.");

        var hasAttendance = await _unitOfWork.Attendances.ExistsAsync(a => a.CourseAssignmentId == id);
        if (hasAttendance)
            return ApiResponse.Fail("Cannot delete this assignment. Attendance records exist for it.");

        assignment.UpdatedAt = DateTime.UtcNow;
        assignment.UpdatedBy = deletedBy;
        _unitOfWork.CourseAssignments.Delete(assignment);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Assignment deleted successfully.");
    }
}