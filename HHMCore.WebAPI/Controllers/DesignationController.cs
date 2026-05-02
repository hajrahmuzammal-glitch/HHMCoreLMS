using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Designation;
using HHMCore.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HHMCore.WebAPI.Controllers;

[ApiController]
[Route("api/designations")]
[Authorize(Roles = AppRoles.Admin)]
public class DesignationController : ControllerBase
{
    private readonly IDesignationService _designationService;

    public DesignationController(IDesignationService designationService)
    {
        _designationService = designationService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDesignationDto dto)
    {
        var result = await _designationService.CreateAsync(dto, GetCurrentUserEmail());
        return result.Success ? StatusCode(201, result) : BadRequest(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _designationService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _designationService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDesignationDto dto)
    {
        var result = await _designationService.UpdateAsync(id,dto, GetCurrentUserEmail());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _designationService.DeleteAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private string GetCurrentUserEmail() =>
        User.FindFirstValue(ClaimTypes.Email) ?? "system";
}
