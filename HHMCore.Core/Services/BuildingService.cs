using AutoMapper;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Building;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;

namespace HHMCore.Core.Services;

public class BuildingService : IBuildingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BuildingService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<BuildingResponseDto>> CreateAsync(CreateBuildingDto dto, string createdBy)
    {
        var normalizedName = dto.Name.Trim();

        var nameExists = await _unitOfWork.Buildings.ExistsAsync(
            b => b.Name.ToLower() == normalizedName.ToLower());
        if (nameExists)
        {
            return ApiResponse<BuildingResponseDto>.Fail("A building with this name already exists.");
        }

        // Code is optional, but must be unique if provided
        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            var codeExists = await _unitOfWork.Buildings.ExistsAsync(
                b => b.Code!.ToUpper() == dto.Code.ToUpper());
            if (codeExists)
            {
                return ApiResponse<BuildingResponseDto>.Fail("A building with this code already exists.");
            }
        }

        var building = new Building
        {
            Name = normalizedName,
            Code = dto.Code?.Trim().ToUpper(),
            Description = dto.Description?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _unitOfWork.Buildings.AddAsync(building);
        await _unitOfWork.SaveChangesAsync();

        // Load with rooms collection for RoomCount
        var created = await _unitOfWork.Buildings.GetByIdWithIncludesAsync(building.Id, b => b.Rooms);
        var mapped = _mapper.Map<BuildingResponseDto>(created);
        return ApiResponse<BuildingResponseDto>.Ok(mapped, "Building created successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<BuildingResponseDto>>> GetAllAsync()
    {
        var buildings = await _unitOfWork.Buildings.GetAllWithIncludesAsync(b => b.Rooms);
        var mapped = _mapper.Map<IReadOnlyList<BuildingResponseDto>>(buildings);
        return ApiResponse<IReadOnlyList<BuildingResponseDto>>.Ok(mapped, "Buildings fetched.");
    }

    public async Task<ApiResponse<BuildingResponseDto>> GetByIdAsync(Guid id)
    {
        var building = await _unitOfWork.Buildings.GetByIdWithIncludesAsync(id, b => b.Rooms);
        if (building == null)
        {
            return ApiResponse<BuildingResponseDto>.Fail("Building not found.");
        }

        var mapped = _mapper.Map<BuildingResponseDto>(building);
        return ApiResponse<BuildingResponseDto>.Ok(mapped, "Building fetched.");
    }

    public async Task<ApiResponse<BuildingResponseDto>> UpdateAsync(
 Guid id,
 UpdateBuildingDto dto,
 string updatedBy)
    {
        // Step 1 — Fetch with Rooms included (needed for response mapping)
        var building = await _unitOfWork.Buildings.GetByIdWithIncludesAsync(id, b => b.Rooms);
        if (building == null)
        {
            return ApiResponse<BuildingResponseDto>.Fail("Building not found.");
        }

        // Step 2 — Apply only fields that were actually sent
        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var nameExists = await _unitOfWork.Buildings.ExistsAsync(
                b => b.Name.ToLower() == dto.Name.ToLower() && b.Id != id);
            if (nameExists)
            {
                return ApiResponse<BuildingResponseDto>.Fail(
                    $"A building with the name '{dto.Name}' already exists.");
            }

            building.Name = dto.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            var codeExists = await _unitOfWork.Buildings.ExistsAsync(
                b => b.Code!.ToUpper() == dto.Code.ToUpper() && b.Id != id);
            if (codeExists)
            {
                return ApiResponse<BuildingResponseDto>.Fail(
                    $"A building with the code '{dto.Code.ToUpper()}' already exists.");
            }

            building.Code = dto.Code.Trim().ToUpper();
        }

        if (!string.IsNullOrWhiteSpace(dto.Description))
        {
            building.Description = dto.Description.Trim();
        }

        // bool? — use ?? because bool cannot be empty string
        building.IsActive = dto.IsActive ?? building.IsActive;

        // Step 3 — Stamp who updated it and when
        building.UpdatedAt = DateTime.UtcNow;
        building.UpdatedBy = updatedBy;

        // Step 4 — Save (no need to re-fetch — EF tracks changes on the loaded entity)
        _unitOfWork.Buildings.Update(building);
        await _unitOfWork.SaveChangesAsync();

        // Step 5 — Map the already-loaded entity and return
        var responseDto = _mapper.Map<BuildingResponseDto>(building);
        return ApiResponse<BuildingResponseDto>.Ok(responseDto, "Building updated successfully.");
    }
    public async Task<ApiResponse> DeleteAsync(Guid id, string deletedBy)
    {
        var building = await _unitOfWork.Buildings.GetByIdAsync(id);
        if (building == null)
        {
            return ApiResponse.Fail("Building not found.");
        }

        // Cannot delete a building that still has rooms
        var hasRooms = await _unitOfWork.Rooms.ExistsAsync(r => r.BuildingId == id);
        if (hasRooms)
        {
            return ApiResponse.Fail("Cannot delete a building that has rooms assigned. Remove rooms first.");
        }

        building.UpdatedAt = DateTime.UtcNow;
        building.UpdatedBy = deletedBy;

        _unitOfWork.Buildings.Delete(building);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Building deleted successfully.");
    }
}
