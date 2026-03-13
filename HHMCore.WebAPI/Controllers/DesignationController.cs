using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Designation;
using HHMCore.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HHMCore.WebAPI.Controllers;

[ApiController]
[Route("api/designations")]
[Authorize(Roles = "Admin")]
public class DesignationController : ControllerBase
{
    private readonly IDesignationService _designationService;

    public DesignationController(IDesignationService designationService)
    {
        _designationService = designationService;
    }

    private string GetCurrentUserEmail() =>
        User.FindFirstValue(ClaimTypes.Email) ?? "system";

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDesignationDto dto)
    {
        var result = await _designationService.CreateAsync(dto, GetCurrentUserEmail());
        return Ok(ApiResponse<DesignationResponseDto>.Ok(result, "Designation created successfully."));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _designationService.GetAllAsync();
        return Ok(ApiResponse<List<DesignationResponseDto>>.Ok(result, "Designations retrieved successfully."));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _designationService.GetByIdAsync(id);
        return Ok(ApiResponse<DesignationResponseDto>.Ok(result, "Designation retrieved successfully."));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDesignationDto dto)
    {
        var result = await _designationService.UpdateAsync(id, dto, GetCurrentUserEmail());
        return Ok(ApiResponse<DesignationResponseDto>.Ok(result, "Designation updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _designationService.DeleteAsync(id, GetCurrentUserEmail());
        return Ok(ApiResponse.Ok("Designation deleted successfully."));
    }
}