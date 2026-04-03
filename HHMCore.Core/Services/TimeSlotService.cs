using AutoMapper;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.TimeSlot;
using HHMCore.Core.Entities;
using HHMCore.Core.Enums;
using HHMCore.Core.Interfaces;

namespace HHMCore.Core.Services;

public class TimeSlotService : ITimeSlotService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TimeSlotService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<TimeSlotResponseDto>> CreateAsync(CreateTimeSlotDto dto, string createdBy)
    {
        var exists = await _unitOfWork.TimeSlots.ExistsAsync(
            ts => ts.Days == dto.Days &&
                  ts.StartTime == dto.StartTime &&
                  ts.EndTime == dto.EndTime);

        if (exists)
            return ApiResponse<TimeSlotResponseDto>.Fail("A time slot with the same days and times already exists.");

        var label = GenerateLabel(dto.Days, dto.StartTime, dto.EndTime);

        var timeSlot = new TimeSlot
        {
            Id = Guid.NewGuid(),
            Days = dto.Days,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Label = label,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _unitOfWork.TimeSlots.AddAsync(timeSlot);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<TimeSlotResponseDto>.Ok(_mapper.Map<TimeSlotResponseDto>(timeSlot), "Time slot created successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<TimeSlotResponseDto>>> GetAllAsync()
    {
        var slots = await _unitOfWork.TimeSlots.GetAllAsync();
        return ApiResponse<IReadOnlyList<TimeSlotResponseDto>>.Ok(_mapper.Map<IReadOnlyList<TimeSlotResponseDto>>(slots), "Time slots fetched.");
    }

    public async Task<ApiResponse<TimeSlotResponseDto>> GetByIdAsync(Guid id)
    {
        var slot = await _unitOfWork.TimeSlots.GetByIdAsync(id);
        if (slot is null)
            return ApiResponse<TimeSlotResponseDto>.Fail("Time slot not found.");

        return ApiResponse<TimeSlotResponseDto>.Ok(_mapper.Map<TimeSlotResponseDto>(slot), "Time slot fetched.");
    }

    public async Task<ApiResponse<TimeSlotResponseDto>> UpdateAsync(Guid id, UpdateTimeSlotDto dto, string updatedBy)
    {
        var slot = await _unitOfWork.TimeSlots.GetByIdAsync(id);
        if (slot is null)
            return ApiResponse<TimeSlotResponseDto>.Fail("Time slot not found.");

        var newDays = dto.Days ?? slot.Days;
        var newStart = dto.StartTime ?? slot.StartTime;
        var newEnd = dto.EndTime ?? slot.EndTime;

        var duplicate = await _unitOfWork.TimeSlots.ExistsAsync(
            ts => ts.Id != id &&
                  ts.Days == newDays &&
                  ts.StartTime == newStart &&
                  ts.EndTime == newEnd);

        if (duplicate)
            return ApiResponse<TimeSlotResponseDto>.Fail("A time slot with the same days and times already exists.");

        slot.Days = newDays;
        slot.StartTime = newStart;
        slot.EndTime = newEnd;
        slot.Label = GenerateLabel(newDays, newStart, newEnd);
        slot.UpdatedAt = DateTime.UtcNow;
        slot.UpdatedBy = updatedBy;

        _unitOfWork.TimeSlots.Update(slot);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<TimeSlotResponseDto>.Ok(_mapper.Map<TimeSlotResponseDto>(slot), "Time slot updated successfully.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, string deletedBy)
    {
        var slot = await _unitOfWork.TimeSlots.GetByIdAsync(id);
        if (slot is null)
            return ApiResponse.Fail("Time slot not found.");

        var hasAssignments = await _unitOfWork.CourseAssignments.ExistsAsync(ca => ca.TimeSlotId == id);
        if (hasAssignments)
            return ApiResponse.Fail("Cannot delete this time slot. It is assigned to one or more classes. Reassign the classes first.");

        slot.UpdatedAt = DateTime.UtcNow;
        slot.UpdatedBy = deletedBy;
        _unitOfWork.TimeSlots.Delete(slot);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Time slot deleted successfully.");
    }

    private static string GenerateLabel(LmsDaysOfWeek days, TimeOnly start, TimeOnly end)
    {
        var map = new Dictionary<LmsDaysOfWeek, string>
        {
            { LmsDaysOfWeek.Monday,    "Mon" },
            { LmsDaysOfWeek.Tuesday,   "Tue" },
            { LmsDaysOfWeek.Wednesday, "Wed" },
            { LmsDaysOfWeek.Thursday,  "Thu" },
            { LmsDaysOfWeek.Friday,    "Fri" },
            { LmsDaysOfWeek.Saturday,  "Sat" }
        };

        var dayString = string.Join("/", map.Where(kvp => days.HasFlag(kvp.Key)).Select(kvp => kvp.Value));
        return $"{dayString} {start:HH:mm}–{end:HH:mm}";
    }
}