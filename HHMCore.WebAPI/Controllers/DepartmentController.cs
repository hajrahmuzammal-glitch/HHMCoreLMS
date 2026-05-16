
using System.Security.Claims;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Department;
using HHMCore.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HHMCore.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
    {
        var result = await _departmentService.CreateAsync(dto, GetCurrentUser());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _departmentService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _departmentService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentDto dto)
    {
        var result = await _departmentService.UpdateAsync(id, dto, GetCurrentUser());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deletedBy = GetCurrentUser();
        var result = await _departmentService.DeleteAsync(id, deletedBy);
        return result.Success ? Ok(result) : NotFound(result);
    }

    //Private Helper Method to get current user email from claims
    // Reads the logged-in user's email from their JWT token
    private string GetCurrentUser() =>
        User.FindFirstValue(ClaimTypes.Email) ?? "system";
}
