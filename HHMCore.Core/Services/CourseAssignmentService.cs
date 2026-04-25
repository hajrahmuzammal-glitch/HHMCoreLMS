// HHMCore.Core/Services/CourseAssignmentService.cs

using AutoMapper;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.CourseAssignment;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    // ── Shared include chain — used by GetById, GetAll, GetBySemester ─────────
    // Defines the full navigation property tree needed for CourseAssignmentResponseDto
    private static Func<IQueryable<CourseAssignment>, IQueryable<CourseAssignment>> FullIncludes()
        => q => q
            .Include(ca => ca.Course)
            .Include(ca => ca.Department)
            .Include(ca => ca.Teacher).ThenInclude(t => t.User)
            .Include(ca => ca.Room).ThenInclude(r => r.Building)
            .Include(ca => ca.TimeSlot)
            .Include(ca => ca.Semester);

    // ── CREATE ────────────────────────────────────────────────────────────────

    public async Task<ApiResponse<CourseAssignmentResponseDto>> CreateAsync(
        CreateCourseAssignmentDto dto,
        string createdBy)
    {
        // 1. Verify Teacher exists and is active
        var teacher = await _unitOfWork.Teachers.GetByIdAsync(dto.TeacherId);
        if (teacher is null || teacher.IsDeleted)
        {
            return ApiResponse<CourseAssignmentResponseDto>.Fail("Teacher not found.");
        }
        // 2. Verify Course exists, is active, get DepartmentId
        var course = await _unitOfWork.Courses.GetByIdAsync(dto.CourseId);
        if (course is null || course.IsDeleted)
        {
            return ApiResponse<CourseAssignmentResponseDto>.Fail("Course not found.");
        }
        // 3. Verify Semester exists and is active
        var semester = await _unitOfWork.Semesters.GetByIdAsync(dto.SemesterId);
        if (semester is null || semester.IsDeleted)
        {
            return ApiResponse<CourseAssignmentResponseDto>.Fail("Semester not found.");
        }
        // 4. Verify Room exists, get capacity for validation
        var room = await _unitOfWork.Rooms.GetByIdAsync(dto.RoomId);
        if (room is null || room.IsDeleted)
        {
            return ApiResponse<CourseAssignmentResponseDto>.Fail("Room not found.");
        }
        // 5. Verify TimeSlot exists
        var timeSlotExists = await _unitOfWork.TimeSlots.ExistsAsync(
            ts => ts.Id == dto.TimeSlotId && !ts.IsDeleted);
        if (!timeSlotExists)
        {
            return ApiResponse<CourseAssignmentResponseDto>.Fail("Time slot not found.");
        }
        // 6. MaxEnrollment cannot exceed physical room capacity
        if (dto.MaxEnrollment > room.Capacity)
        {
            return ApiResponse<CourseAssignmentResponseDto>.Fail(
                $"Max enrollment ({dto.MaxEnrollment}) cannot exceed room capacity ({room.Capacity}).");
        }
        // 7. CONFLICT RULE 1 — Room already booked at this time in this semester
        var roomConflict = await _unitOfWork.CourseAssignments.ExistsAsync(
            ca => ca.RoomId == dto.RoomId &&
                  ca.TimeSlotId == dto.TimeSlotId &&
                  ca.SemesterId == dto.SemesterId &&
                  !ca.IsDeleted);
        if (roomConflict)
        {
            return ApiResponse<CourseAssignmentResponseDto>.Fail(
                "This room is already booked for this time slot in this semester.");
        }
        // 8. CONFLICT RULE 2 — Teacher already teaching at this time in this semester
        var teacherConflict = await _unitOfWork.CourseAssignments.ExistsAsync(
            ca => ca.TeacherId == dto.TeacherId &&
                  ca.TimeSlotId == dto.TimeSlotId &&
                  ca.SemesterId == dto.SemesterId &&
                  !ca.IsDeleted);
        if (teacherConflict)
        {
            return ApiResponse<CourseAssignmentResponseDto>.Fail(
                "This teacher already has a class at this time slot in this semester.");
        }
        // 9. CONFLICT RULE 3 — Same course already scheduled at this time in this semester
        var courseConflict = await _unitOfWork.CourseAssignments.ExistsAsync(
            ca => ca.CourseId == dto.CourseId &&
                  ca.TimeSlotId == dto.TimeSlotId &&
                  ca.SemesterId == dto.SemesterId &&
                  !ca.IsDeleted);
        if (courseConflict)
        {
            return ApiResponse<CourseAssignmentResponseDto>.Fail(
                "This course is already scheduled at this time slot in this semester.");
        }
        // 10. CONFLICT RULE 4 — Same department + section already has a class at this time
        //     Uses DepartmentId from Course — this is why we fetch Course above
        var sectionConflict = await _unitOfWork.CourseAssignments.ExistsAsync(
            ca => ca.DepartmentId == course.DepartmentId &&
                  ca.Section == dto.Section &&
                  ca.TimeSlotId == dto.TimeSlotId &&
                  ca.SemesterId == dto.SemesterId &&
                  !ca.IsDeleted);
        if (sectionConflict)
        {
            return ApiResponse<CourseAssignmentResponseDto>.Fail(
                "This section already has a class scheduled at this time slot.");
        }
        // 11. Save — DepartmentId denormalized from Course
        var assignment = new CourseAssignment
        {
            Id = Guid.NewGuid(),
            TeacherId = dto.TeacherId,
            CourseId = dto.CourseId,
            SemesterId = dto.SemesterId,
            RoomId = dto.RoomId,
            TimeSlotId = dto.TimeSlotId,
            DepartmentId = course.DepartmentId,   // derived — not from DTO
            Section = dto.Section,
            MaxEnrollment = dto.MaxEnrollment,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _unitOfWork.CourseAssignments.AddAsync(assignment);
        await _unitOfWork.SaveChangesAsync();

        // 12. Re-fetch with full include chain — navigation props null after save
        var saved = await _unitOfWork.CourseAssignments
            .GetByIdWithDetailsAsync(assignment.Id, FullIncludes());

        return ApiResponse<CourseAssignmentResponseDto>.Ok(
            _mapper.Map<CourseAssignmentResponseDto>(saved),
            "Course assignment created successfully.");
    }

    // ── GET ALL ───────────────────────────────────────────────────────────────

    public async Task<ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>> GetAllAsync()
    {
        var assignments = await _unitOfWork.CourseAssignments
            .GetAllWithDetailsAsync(FullIncludes());

        return ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>.Ok(
            _mapper.Map<IReadOnlyList<CourseAssignmentResponseDto>>(assignments),
            "Course assignments fetched.");
    }

    // ── GET BY SEMESTER ───────────────────────────────────────────────────────

    public async Task<ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>> GetBySemesterAsync(
        Guid semesterId)
    {
        var assignments = await _unitOfWork.CourseAssignments
            .FindWithDetailsAsync(
                ca => ca.SemesterId == semesterId,
                FullIncludes());

        return ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>.Ok(
            _mapper.Map<IReadOnlyList<CourseAssignmentResponseDto>>(assignments),
            "Semester timetable fetched.");
    }

    // ── GET BY ID ─────────────────────────────────────────────────────────────

    public async Task<ApiResponse<CourseAssignmentResponseDto>> GetByIdAsync(Guid id)
    {
        var assignment = await _unitOfWork.CourseAssignments
            .GetByIdWithDetailsAsync(id, FullIncludes());

        if (assignment is null)
        {
            return ApiResponse<CourseAssignmentResponseDto>.Fail(
                "Course assignment not found.");
        }
        return ApiResponse<CourseAssignmentResponseDto>.Ok(
            _mapper.Map<CourseAssignmentResponseDto>(assignment),
            "Course assignment fetched.");
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────

    public async Task<ApiResponse<CourseAssignmentResponseDto>> UpdateAsync(
        Guid id,
        UpdateCourseAssignmentDto dto,
        string updatedBy)
    {
        // Fetch existing — need current values to resolve finals and run conflict checks
        var assignment = await _unitOfWork.CourseAssignments.GetByIdAsync(id);
        if (assignment is null)
        {
            return ApiResponse<CourseAssignmentResponseDto>.Fail(
                "Course assignment not found.");
        }
        // Resolve final values — null in DTO means keep existing
        var finalTeacherId = dto.TeacherId ?? assignment.TeacherId;
        var finalRoomId = dto.RoomId ?? assignment.RoomId;
        var finalTimeSlotId = dto.TimeSlotId ?? assignment.TimeSlotId;
        var finalSection = string.IsNullOrWhiteSpace(dto.Section)
                                ? assignment.Section
                                : dto.Section;
        var finalMaxEnroll = dto.MaxEnrollment ?? assignment.MaxEnrollment;

        // Validate changed entities only — skip unchanged to avoid unnecessary DB calls
        Room? room = null;

        if (dto.TeacherId.HasValue)
        {
            var teacherExists = await _unitOfWork.Teachers.ExistsAsync(
                t => t.Id == dto.TeacherId && !t.IsDeleted);
            if (!teacherExists)
            {
                return ApiResponse<CourseAssignmentResponseDto>.Fail("Teacher not found.");
            }
        }

        if (dto.RoomId.HasValue || dto.MaxEnrollment.HasValue)
        {
            // Fetch room to validate capacity — needed if room OR enrollment changed
            room = await _unitOfWork.Rooms.GetByIdAsync(finalRoomId);
            if (room is null || room.IsDeleted)
            {
                return ApiResponse<CourseAssignmentResponseDto>.Fail("Room not found.");
            }
            if (finalMaxEnroll > room.Capacity)
            {
                return ApiResponse<CourseAssignmentResponseDto>.Fail(
                    $"Max enrollment ({finalMaxEnroll}) cannot exceed room capacity ({room.Capacity}).");
            }
        }

        if (dto.TimeSlotId.HasValue)
        {
            var timeSlotExists = await _unitOfWork.TimeSlots.ExistsAsync(
                ts => ts.Id == dto.TimeSlotId && !ts.IsDeleted);
            if (!timeSlotExists)
            {
                return ApiResponse<CourseAssignmentResponseDto>.Fail("Time slot not found.");
            }
        }

        // Conflict checks — each runs independently based on what actually changed
        // Every check excludes the current record (ca.Id != id)

        // CONFLICT RULE 1 — Room conflict: run if room OR timeslot changed
        if (dto.RoomId.HasValue || dto.TimeSlotId.HasValue)
        {
            var roomConflict = await _unitOfWork.CourseAssignments.ExistsAsync(
                ca => ca.RoomId == finalRoomId &&
                      ca.TimeSlotId == finalTimeSlotId &&
                      ca.SemesterId == assignment.SemesterId &&
                      ca.Id != id &&
                      !ca.IsDeleted);
            if (roomConflict)
            {
                return ApiResponse<CourseAssignmentResponseDto>.Fail(
                    "This room is already booked for this time slot in this semester.");
            }
        }

        // CONFLICT RULE 2 — Teacher conflict: run if teacher OR timeslot changed
        if (dto.TeacherId.HasValue || dto.TimeSlotId.HasValue)
        {
            var teacherConflict = await _unitOfWork.CourseAssignments.ExistsAsync(
                ca => ca.TeacherId == finalTeacherId &&
                      ca.TimeSlotId == finalTimeSlotId &&
                      ca.SemesterId == assignment.SemesterId &&
                      ca.Id != id &&
                      !ca.IsDeleted);
            if (teacherConflict)
            {
                return ApiResponse<CourseAssignmentResponseDto>.Fail(
                    "This teacher already has a class at this time slot in this semester.");
            }
        }

        // CONFLICT RULE 3 — Section conflict: run if section OR timeslot changed
        if (dto.Section != null || dto.TimeSlotId.HasValue)
        {
            var sectionConflict = await _unitOfWork.CourseAssignments.ExistsAsync(
                ca => ca.DepartmentId == assignment.DepartmentId &&
                      ca.Section == finalSection &&
                      ca.TimeSlotId == finalTimeSlotId &&
                      ca.SemesterId == assignment.SemesterId &&
                      ca.Id != id &&
                      !ca.IsDeleted);
            if (sectionConflict)
            {
                return ApiResponse<CourseAssignmentResponseDto>.Fail(
                    "This section already has a class scheduled at this time slot.");
            }
        }

        // Apply changes
        assignment.TeacherId = finalTeacherId;
        assignment.RoomId = finalRoomId;
        assignment.TimeSlotId = finalTimeSlotId;
        assignment.Section = finalSection;
        assignment.MaxEnrollment = finalMaxEnroll;
        assignment.UpdatedAt = DateTime.UtcNow;
        assignment.UpdatedBy = updatedBy;

        _unitOfWork.CourseAssignments.Update(assignment);
        await _unitOfWork.SaveChangesAsync();

        // Re-fetch with full include chain
        var saved = await _unitOfWork.CourseAssignments
            .GetByIdWithDetailsAsync(assignment.Id, FullIncludes());

        return ApiResponse<CourseAssignmentResponseDto>.Ok(
            _mapper.Map<CourseAssignmentResponseDto>(saved),
            "Course assignment updated successfully.");
    }

    // ── DELETE (soft) ─────────────────────────────────────────────────────────

    public async Task<ApiResponse> DeleteAsync(Guid id, string deletedBy)
    {
        var assignment = await _unitOfWork.CourseAssignments.GetByIdAsync(id);
        if (assignment is null)
        {
            return ApiResponse.Fail("Course assignment not found.");
        }
        // Block delete if active attendance records exist
        var hasAttendance = await _unitOfWork.Attendances.ExistsAsync(
            a => a.CourseAssignmentId == id && !a.IsDeleted);
        if (hasAttendance)
        {
            return ApiResponse.Fail(
                "Cannot delete — attendance records exist for this assignment. Archive the semester first.");
        }
        assignment.IsDeleted = true;
        assignment.UpdatedAt = DateTime.UtcNow;
        assignment.UpdatedBy = deletedBy;

        _unitOfWork.CourseAssignments.Update(assignment);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Course assignment deleted successfully.");
    }
    // ── GET BY TEACHER ────────────────────────────────────────────────────────

    public async Task<ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>> GetByTeacherAsync(
        Guid teacherId)
    {
        var teacherExists = await _unitOfWork.Teachers.ExistsAsync(
            t => t.Id == teacherId && !t.IsDeleted);
        if (!teacherExists)
        {
            return ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>.Fail("Teacher not found.");
        }
        var assignments = await _unitOfWork.CourseAssignments
            .FindWithDetailsAsync(
                ca => ca.TeacherId == teacherId,
                FullIncludes());

        return ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>.Ok(
            _mapper.Map<IReadOnlyList<CourseAssignmentResponseDto>>(assignments),
            "Teacher assignments fetched.");
    }

    // ── GET MY ASSIGNMENTS (current logged-in teacher) ────────────────────────

    public async Task<ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>> GetMyAssignmentsAsync(
        string userId)
    {
        var teacher = await _unitOfWork.Teachers.FindOneAsync(
            t => t.UserId == userId && !t.IsDeleted);
        if (teacher is null)
        {
            return ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>.Fail("Teacher profile not found.");
        }

        var assignments = await _unitOfWork.CourseAssignments
            .FindWithDetailsAsync(
                ca => ca.TeacherId == teacher.Id,
                FullIncludes());

        return ApiResponse<IReadOnlyList<CourseAssignmentResponseDto>>.Ok(
            _mapper.Map<IReadOnlyList<CourseAssignmentResponseDto>>(assignments),
            "Your assignments fetched.");
    }
}
