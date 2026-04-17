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
    Guid id,
    UpdateRoomDto dto,
    string updatedBy)
    {
        // Step 1 — Fetch with Building included (needed for response mapping)
        var room = await _unitOfWork.Rooms.GetByIdWithIncludesAsync(id, r => r.Building);
        if (room == null)
            return ApiResponse<RoomResponseDto>.Fail("Room not found.");

        // Step 2 — If a new BuildingId was sent, verify that building exists
        if (dto.BuildingId.HasValue)
        {
            var building = await _unitOfWork.Buildings.GetByIdAsync(dto.BuildingId.Value);
            if (building == null)
                return ApiResponse<RoomResponseDto>.Fail("Building not found.");
        }

        // Step 3 — Duplicate room number check
        // Use the incoming BuildingId if provided, otherwise keep the current one
        if (!string.IsNullOrWhiteSpace(dto.RoomNumber))
        {
            var targetBuildingId = dto.BuildingId ?? room.BuildingId;
            var roomExists = await _unitOfWork.Rooms.ExistsAsync(
                r => r.RoomNumber.ToLower() == dto.RoomNumber.ToLower()
                  && r.BuildingId == targetBuildingId
                  && r.Id != id);
            if (roomExists)
                return ApiResponse<RoomResponseDto>.Fail(
                    $"Room '{dto.RoomNumber}' already exists in this building.");

            room.RoomNumber = dto.RoomNumber.Trim();
        }

        // Step 4 — Apply only the fields that were actually sent
        // ?? handles value types (Guid?, int?, bool?, enum?) — null means "don't change"
        room.BuildingId = dto.BuildingId ?? room.BuildingId;
        room.Capacity = dto.Capacity ?? room.Capacity;
        room.RoomType = dto.RoomType ?? room.RoomType;
        room.IsActive = dto.IsActive ?? room.IsActive;

        // Step 5 — Stamp who updated it and when
        room.UpdatedAt = DateTime.UtcNow;
        room.UpdatedBy = updatedBy;

        // Step 6 — Save (no re-fetch needed — EF tracks changes on the loaded entity)
        _unitOfWork.Rooms.Update(room);
        await _unitOfWork.SaveChangesAsync();

        // Step 7 — Map the already-loaded entity (Building navigation is already there)
        var responseDto = _mapper.Map<RoomResponseDto>(room);
        return ApiResponse<RoomResponseDto>.Ok(responseDto, "Room updated successfully.");
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