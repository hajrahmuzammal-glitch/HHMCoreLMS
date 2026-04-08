using AutoMapper;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Room;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;

namespace HHMCore.Core.Services;

public class RoomService : IRoomService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RoomService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<RoomResponseDto>> CreateAsync(CreateRoomDto dto, string createdBy)
    {
        var exists = await _unitOfWork.Rooms.ExistsAsync(
            r => r.RoomNumber.ToUpper() == dto.RoomNumber.ToUpper() &&
                 r.BuildingId == dto.BuildingId);

        if (exists)
            return ApiResponse<RoomResponseDto>.Fail($"Room '{dto.RoomNumber}' in building already exists.");

        var room = new Room
        {
            RoomNumber = dto.RoomNumber.Trim(),
            BuildingId = dto.BuildingId,
            Capacity = dto.Capacity,
            RoomType = dto.RoomType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _unitOfWork.Rooms.AddAsync(room);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<RoomResponseDto>.Ok(_mapper.Map<RoomResponseDto>(room), "Room created successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<RoomResponseDto>>> CreateBulkAsync(
        List<CreateRoomDto> dtos, string createdBy)
    {
        if (dtos == null || dtos.Count == 0)
            return ApiResponse<IReadOnlyList<RoomResponseDto>>.Fail("No rooms provided.");

        var createdRooms = new List<Room>();
        var skipped = new List<string>();

        foreach (var dto in dtos)
        {
            var exists = await _unitOfWork.Rooms.ExistsAsync(
                r => r.RoomNumber.ToUpper() == dto.RoomNumber.ToUpper() &&
                     r.BuildingId == dto.BuildingId);

            if (exists)
            {
                skipped.Add($"{dto.RoomNumber} (BuildingId: {dto.BuildingId})");
                continue;
            }

            var room = new Room
            {
                RoomNumber = dto.RoomNumber.Trim(),
                BuildingId = dto.BuildingId,
                Capacity = dto.Capacity,
                RoomType = dto.RoomType,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            await _unitOfWork.Rooms.AddAsync(room);
            createdRooms.Add(room);
        }

        await _unitOfWork.SaveChangesAsync();

        var result = _mapper.Map<IReadOnlyList<RoomResponseDto>>(createdRooms);
        var message = skipped.Count > 0
            ? $"{createdRooms.Count} room(s) created. Skipped duplicates: {string.Join(", ", skipped)}."
            : $"{createdRooms.Count} room(s) created successfully.";

        return ApiResponse<IReadOnlyList<RoomResponseDto>>.Ok(result, message);
    }

    public async Task<ApiResponse<IReadOnlyList<RoomResponseDto>>> GetAllAsync()
    {
        var rooms = await _unitOfWork.Rooms.GetAllAsync();
        return ApiResponse<IReadOnlyList<RoomResponseDto>>.Ok(
            _mapper.Map<IReadOnlyList<RoomResponseDto>>(rooms), "Rooms fetched.");
    }

    public async Task<ApiResponse<RoomResponseDto>> GetByIdAsync(Guid id)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(id);
        if (room is null)
            return ApiResponse<RoomResponseDto>.Fail("Room not found.");

        return ApiResponse<RoomResponseDto>.Ok(_mapper.Map<RoomResponseDto>(room), "Room fetched.");
    }

    public async Task<ApiResponse<IReadOnlyList<RoomResponseDto>>> GetAvailableRoomsAsync(
        Guid timeSlotId, Guid semesterId)
    {
        var bookedAssignments = await _unitOfWork.CourseAssignments.FindAsync(
            ca => ca.TimeSlotId == timeSlotId && ca.SemesterId == semesterId);

        var bookedRoomIds = bookedAssignments.Select(ca => ca.RoomId).ToHashSet();

        var allRooms = await _unitOfWork.Rooms.FindAsync(r => r.IsActive);

        var availableRooms = allRooms.Where(r => !bookedRoomIds.Contains(r.Id)).ToList();

        return ApiResponse<IReadOnlyList<RoomResponseDto>>.Ok(
            _mapper.Map<IReadOnlyList<RoomResponseDto>>(availableRooms),
            "Available rooms fetched.");
    }

    public async Task<ApiResponse<RoomResponseDto>> UpdateAsync(
        Guid id, UpdateRoomDto dto, string updatedBy)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(id);
        if (room is null)
            return ApiResponse<RoomResponseDto>.Fail("Room not found.");

        if (dto.BuildingId.HasValue)
        {
            var building = await _unitOfWork.Buildings.GetByIdAsync(dto.BuildingId.Value);
            if (building is null)
                return ApiResponse<RoomResponseDto>.Fail("Building not found.");
            room.BuildingId = dto.BuildingId.Value;
        }
        if (!string.IsNullOrWhiteSpace(dto.RoomNumber) || (dto.BuildingId.HasValue && dto.BuildingId != Guid.Empty))
        {
            var newRoomNumber = string.IsNullOrWhiteSpace(dto.RoomNumber) ? room.RoomNumber : dto.RoomNumber;
            var newBuildingId = (dto.BuildingId.HasValue && dto.BuildingId != Guid.Empty)
                ? dto.BuildingId.Value
                : room.BuildingId;

            var duplicate = await _unitOfWork.Rooms.ExistsAsync(
                r => r.Id != id &&
                     r.RoomNumber.ToUpper() == newRoomNumber.ToUpper() &&
                     r.BuildingId == newBuildingId);

            if (duplicate)
                return ApiResponse<RoomResponseDto>.Fail(
                    $"Room '{newRoomNumber}' in this building already exists.");
        }

        room.RoomNumber = string.IsNullOrWhiteSpace(dto.RoomNumber) ? room.RoomNumber : dto.RoomNumber.Trim();
        room.BuildingId = (dto.BuildingId.HasValue && dto.BuildingId != Guid.Empty)
            ? dto.BuildingId.Value
            : room.BuildingId;
        room.Capacity = dto.Capacity ?? room.Capacity;
        room.RoomType = dto.RoomType ?? room.RoomType;
        room.IsActive = dto.IsActive ?? room.IsActive;
        room.UpdatedAt = DateTime.UtcNow;
        room.UpdatedBy = updatedBy;

        _unitOfWork.Rooms.Update(room);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<RoomResponseDto>.Ok(_mapper.Map<RoomResponseDto>(room), "Room updated successfully.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id, string deletedBy)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(id);
        if (room is null)
            return ApiResponse.Fail("Room not found.");

        var hasAssignments = await _unitOfWork.CourseAssignments.ExistsAsync(ca => ca.RoomId == id);
        if (hasAssignments)
            return ApiResponse.Fail(
                "Cannot delete this room. It is assigned to one or more classes. Reassign the classes first.");

        room.UpdatedAt = DateTime.UtcNow;
        room.UpdatedBy = deletedBy;
        _unitOfWork.Rooms.Delete(room);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Room deleted successfully.");
    }
}