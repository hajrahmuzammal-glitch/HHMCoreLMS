using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
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

    public async Task<DesignationResponseDto> CreateAsync(CreateDesignationDto dto, string createdBy)
    {
        var exists = await _unitOfWork.Designations.ExistsAsync(
            x => x.Title.ToLower() == dto.Title.ToLower());

        if (exists)
            throw new InvalidOperationException($"Designation '{dto.Title}' already exists.");

        var designation = _mapper.Map<Designation>(dto);
        designation.CreatedBy = createdBy;

        await _unitOfWork.Designations.AddAsync(designation);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<DesignationResponseDto>(designation);
    }

    public async Task<List<DesignationResponseDto>> GetAllAsync()
    {
        var designations = await _unitOfWork.Designations.GetAllAsync();
        return _mapper.Map<List<DesignationResponseDto>>(designations);
    }

    public async Task<DesignationResponseDto> GetByIdAsync(Guid id)
    {
        var designation = await _unitOfWork.Designations.GetByIdAsync(id);

        if (designation == null)
            throw new KeyNotFoundException($"Designation with ID '{id}' was not found.");

        return _mapper.Map<DesignationResponseDto>(designation);
    }

    public async Task<DesignationResponseDto> UpdateAsync(Guid id, UpdateDesignationDto dto, string updatedBy)
    {
        var designation = await _unitOfWork.Designations.GetByIdAsync(id);

        if (designation == null)
            throw new KeyNotFoundException($"Designation with ID '{id}' was not found.");

        if (dto.Title != null)
        {
            var titleExists = await _unitOfWork.Designations.ExistsAsync(
                x => x.Title.ToLower() == dto.Title.ToLower() && x.Id != id);

            if (titleExists)
                throw new InvalidOperationException($"Designation '{dto.Title}' already exists.");

            designation.Title = dto.Title;
        }

        if (dto.Description != null)
            designation.Description = dto.Description;

        designation.UpdatedBy = updatedBy;
        designation.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Designations.Update(designation);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<DesignationResponseDto>(designation);
    }

    public async Task DeleteAsync(Guid id, string deletedBy)
    {
        var designation = await _unitOfWork.Designations.GetByIdAsync(id);

        if (designation == null)
            throw new KeyNotFoundException($"Designation with ID '{id}' was not found.");

        var hasTeachers = await _unitOfWork.Teachers.ExistsAsync(
            x => x.DesignationId == id);

        if (hasTeachers)
            throw new InvalidOperationException(
                "Cannot delete this designation because teachers are assigned to it.");

        designation.IsDeleted = true;
        designation.UpdatedBy = deletedBy;
        designation.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Designations.Delete(designation);
        await _unitOfWork.SaveChangesAsync();
    }
}
