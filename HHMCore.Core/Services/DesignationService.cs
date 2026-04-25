using AutoMapper;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Designation;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;

namespace HHMCore.Core.Services;

public class DesignationService : IDesignationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DesignationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<DesignationResponseDto>> CreateAsync(CreateDesignationDto dto, string createdBy)
    {
        var exists = await _unitOfWork.Designations.ExistsAsync(
            x => x.Title.ToLower() == dto.Title.ToLower());
        if (exists)
        {
            return ApiResponse<DesignationResponseDto>.Fail($"Designation '{dto.Title}' already exists.");
        }

        var designation = new Designation
        {
            Title = dto.Title,
            Description = dto.Description,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Designations.AddAsync(designation);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<DesignationResponseDto>(designation);
        return ApiResponse<DesignationResponseDto>.Ok(response, "Designation created successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<DesignationResponseDto>>> GetAllAsync()
    {
        var designations = await _unitOfWork.Designations.GetAllAsync();
        var response = _mapper.Map<IReadOnlyList<DesignationResponseDto>>(designations);
        return ApiResponse<IReadOnlyList<DesignationResponseDto>>.Ok(response, "Designations retrieved successfully.");
    }

    public async Task<ApiResponse<DesignationResponseDto>> GetByIdAsync(Guid id)
    {
        var designation = await _unitOfWork.Designations.GetByIdAsync(id);
        if (designation == null)
        {
            return ApiResponse<DesignationResponseDto>.Fail("Designation not found.");
        }

        var response = _mapper.Map<DesignationResponseDto>(designation);
        return ApiResponse<DesignationResponseDto>.Ok(response, "Designation retrieved successfully.");
    }

    public async Task<ApiResponse<DesignationResponseDto>> UpdateAsync(
    Guid id,
    UpdateDesignationDto dto,
    string updatedBy)
    {
        // Step 1 — Find the existing record. If it's not there, fail immediately.
        var designation = await _unitOfWork.Designations.GetByIdAsync(id);
        if (designation == null)
        {
            return ApiResponse<DesignationResponseDto>.Fail("Designation not found.");
        }

        // Step 2 — Apply only the fields that were actually sent.
        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            var titleExists = await _unitOfWork.Designations.ExistsAsync(
                d => d.Title.ToLower() == dto.Title.ToLower() && d.Id != id);
            if (titleExists)
            {
                return ApiResponse<DesignationResponseDto>.Fail(
                    $"Designation '{dto.Title}' already exists.");
            }

            designation.Title = dto.Title.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Description))
        {
            designation.Description = dto.Description.Trim();
        }

        // Step 3 — Stamp who updated it and when.
        designation.UpdatedAt = DateTime.UtcNow;
        designation.UpdatedBy = updatedBy;

        // Step 4 — Tell EF Core this record changed, then save.
        _unitOfWork.Designations.Update(designation);
        await _unitOfWork.SaveChangesAsync();

        // Step 5 — Map to response DTO and return.
        var responseDto = _mapper.Map<DesignationResponseDto>(designation);
        return ApiResponse<DesignationResponseDto>.Ok(responseDto, "Designation updated successfully.");
    }
    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        var designation = await _unitOfWork.Designations.GetByIdAsync(id);
        if (designation == null)
        {
            return ApiResponse.Fail("Designation not found.");
        }

        var hasTeachers = await _unitOfWork.Teachers.ExistsAsync(x => x.DesignationId == id);
        if (hasTeachers)
        {
            return ApiResponse.Fail("Cannot delete this designation because teachers are assigned to it.");
        }

        _unitOfWork.Designations.Delete(designation);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Designation deleted successfully.");
    }
}