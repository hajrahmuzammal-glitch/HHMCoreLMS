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
            return ApiResponse<DesignationResponseDto>.Fail($"Designation '{dto.Title}' already exists.");

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
            return ApiResponse<DesignationResponseDto>.Fail("Designation not found.");

        var response = _mapper.Map<DesignationResponseDto>(designation);
        return ApiResponse<DesignationResponseDto>.Ok(response, "Designation retrieved successfully.");
    }

    public async Task<ApiResponse<DesignationResponseDto>> UpdateAsync(UpdateDesignationDto dto, string updatedBy)
    {
        var designation = await _unitOfWork.Designations.GetByIdAsync(dto.Id);
        if (designation == null)
            return ApiResponse<DesignationResponseDto>.Fail("Designation not found.");

        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            var titleExists = await _unitOfWork.Designations.ExistsAsync(
                x => x.Title.ToLower() == dto.Title.ToLower() && x.Id != dto.Id);
            if (titleExists)
                return ApiResponse<DesignationResponseDto>.Fail($"Designation '{dto.Title}' already exists.");

            designation.Title = dto.Title;
        }

        designation.Description = string.IsNullOrWhiteSpace(dto.Description)
            ? designation.Description
            : dto.Description;
        designation.UpdatedBy = updatedBy;
        designation.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Designations.Update(designation);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<DesignationResponseDto>(designation);
        return ApiResponse<DesignationResponseDto>.Ok(response, "Designation updated successfully.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        var designation = await _unitOfWork.Designations.GetByIdAsync(id);
        if (designation == null)
            return ApiResponse.Fail("Designation not found.");

        var hasTeachers = await _unitOfWork.Teachers.ExistsAsync(x => x.DesignationId == id);
        if (hasTeachers)
            return ApiResponse.Fail("Cannot delete this designation because teachers are assigned to it.");

        _unitOfWork.Designations.Delete(designation);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Designation deleted successfully.");
    }
}