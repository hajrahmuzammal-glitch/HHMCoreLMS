using AutoMapper;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Semester;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;

namespace HHMCore.Core.Services;

public class SemesterService : ISemesterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SemesterService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<SemesterResponseDto>> CreateAsync(
        CreateSemesterDto dto, string createdBy)
    {
        var nameExists = await _unitOfWork.Semesters.ExistsAsync(
            s => s.Name.ToLower() == dto.Name.ToLower().Trim());

        if (nameExists)
            return ApiResponse<SemesterResponseDto>.Fail(
                "A semester with this name already exists.");

        var semester = new Semester
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            StartDate = dto.StartDate.ToUniversalTime(),
            EndDate = dto.EndDate.ToUniversalTime(),
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _unitOfWork.Semesters.AddAsync(semester);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<SemesterResponseDto>(semester);
        return ApiResponse<SemesterResponseDto>.Ok(response, "Semester created successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<SemesterResponseDto>>> GetAllAsync()
    {
        var semesters = await _unitOfWork.Semesters.GetAllAsync();
        var response = _mapper.Map<IReadOnlyList<SemesterResponseDto>>(semesters);
        return ApiResponse<IReadOnlyList<SemesterResponseDto>>.Ok(
            response, "Semesters fetched successfully.");
    }

    public async Task<ApiResponse<SemesterResponseDto>> GetByIdAsync(Guid id)
    {
        var semester = await _unitOfWork.Semesters.GetByIdAsync(id);
        if (semester == null)
            return ApiResponse<SemesterResponseDto>.Fail("Semester not found.");

        var response = _mapper.Map<SemesterResponseDto>(semester);
        return ApiResponse<SemesterResponseDto>.Ok(response, "Semester fetched successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<SemesterResponseDto>>> GetActiveAsync()
    {
        var semesters = await _unitOfWork.Semesters.FindAsync(s => s.IsActive);
        var response = _mapper.Map<IReadOnlyList<SemesterResponseDto>>(semesters);
        return ApiResponse<IReadOnlyList<SemesterResponseDto>>.Ok(
            response, "Active semesters fetched successfully.");
    }

    public async Task<ApiResponse<SemesterResponseDto>> UpdateAsync(
        UpdateSemesterDto dto, string updatedBy)
    {
        var semester = await _unitOfWork.Semesters.GetByIdAsync(dto.Id);
        if (semester == null)
            return ApiResponse<SemesterResponseDto>.Fail("Semester not found.");

        if (!string.IsNullOrWhiteSpace(dto.Name) &&
            !string.Equals(dto.Name.Trim(), semester.Name,
                StringComparison.OrdinalIgnoreCase))
        {
            var nameExists = await _unitOfWork.Semesters.ExistsAsync(
                s => s.Name.ToLower() == dto.Name.ToLower().Trim()
                  && s.Id != dto.Id);

            if (nameExists)
                return ApiResponse<SemesterResponseDto>.Fail(
                    "A semester with this name already exists.");
        }

        semester.Name = string.IsNullOrWhiteSpace(dto.Name)
            ? semester.Name : dto.Name.Trim();
        semester.StartDate = dto.StartDate ?? semester.StartDate;
        semester.EndDate = dto.EndDate ?? semester.EndDate;
        semester.UpdatedAt = DateTime.UtcNow;
        semester.UpdatedBy = updatedBy;

        _unitOfWork.Semesters.Update(semester);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<SemesterResponseDto>(semester);
        return ApiResponse<SemesterResponseDto>.Ok(response, "Semester updated successfully.");
    }

    public async Task<ApiResponse<SemesterResponseDto>> ActivateAsync(
        Guid id, string updatedBy)
    {
        var semester = await _unitOfWork.Semesters.GetByIdAsync(id);
        if (semester == null)
            return ApiResponse<SemesterResponseDto>.Fail("Semester not found.");

        if (semester.IsActive)
            return ApiResponse<SemesterResponseDto>.Fail(
                "This semester is already active.");

        semester.IsActive = true;
        semester.UpdatedAt = DateTime.UtcNow;
        semester.UpdatedBy = updatedBy;

        _unitOfWork.Semesters.Update(semester);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<SemesterResponseDto>(semester);
        return ApiResponse<SemesterResponseDto>.Ok(
            response, "Semester activated successfully.");
    }

    public async Task<ApiResponse<SemesterResponseDto>> DeactivateAsync(
        Guid id, string updatedBy)
    {
        var semester = await _unitOfWork.Semesters.GetByIdAsync(id);
        if (semester == null)
            return ApiResponse<SemesterResponseDto>.Fail("Semester not found.");

        if (!semester.IsActive)
            return ApiResponse<SemesterResponseDto>.Fail(
                "This semester is already inactive.");

        semester.IsActive = false;
        semester.UpdatedAt = DateTime.UtcNow;
        semester.UpdatedBy = updatedBy;

        _unitOfWork.Semesters.Update(semester);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<SemesterResponseDto>(semester);
        return ApiResponse<SemesterResponseDto>.Ok(
            response, "Semester deactivated successfully.");
    }

    public async Task<ApiResponse> DeleteAsync(Guid id)
    {
        var semester = await _unitOfWork.Semesters.GetByIdAsync(id);
        if (semester == null)
            return ApiResponse.Fail("Semester not found.");

        if (semester.IsActive)
            return ApiResponse.Fail(
                "Cannot delete an active semester. Deactivate it first.");

        var hasAssignments = await _unitOfWork.CourseAssignments.ExistsAsync(
            ca => ca.SemesterId == id);

        if (hasAssignments)
            return ApiResponse.Fail(
                "Cannot delete this semester. Course assignments are linked to it.");

        var hasEnrollments = await _unitOfWork.Enrollments.ExistsAsync(
            e => e.SemesterId == id);

        if (hasEnrollments)
            return ApiResponse.Fail(
                "Cannot delete this semester. Students are enrolled in it.");

        _unitOfWork.Semesters.Delete(semester);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Semester deleted successfully.");
    }
}