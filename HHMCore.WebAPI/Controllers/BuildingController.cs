using System.Security.Claims;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Building;
using HHMCore.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HHMCore.WebAPI.Controllers;

[ApiController]
[Route("api/buildings")]
[Authorize]
public class BuildingController : ControllerBase
{
    private readonly IBuildingService _buildingService;

    public BuildingController(IBuildingService buildingService)
    {
        _buildingService = buildingService;
    }


    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateBuildingDto dto)
    {
        var createdBy = User.FindFirstValue(ClaimTypes.Email) ?? "system";
        var result = await _buildingService.CreateAsync(dto, createdBy);
        return result.Success ? StatusCode(201, result) : BadRequest(result);
    }


    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _buildingService.GetAllAsync();
        return Ok(result);
    }


    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _buildingService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }


    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBuildingDto dto)
    {
        var updatedBy = User.FindFirstValue(ClaimTypes.Email) ?? "system";
        var result = await _buildingService.UpdateAsync(id, dto, updatedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deletedBy = User.FindFirstValue(ClaimTypes.Email) ?? "system";
        var result = await _buildingService.DeleteAsync(id, deletedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
