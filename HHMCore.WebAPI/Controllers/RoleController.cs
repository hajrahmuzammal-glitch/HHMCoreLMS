using System.Security.Claims;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Role;
using HHMCore.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HHMCore.WebAPI.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = AppRoles.Admin)]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        var createdBy = User.FindFirstValue(ClaimTypes.Email) ?? "system";
        var result = await _roleService.CreateAsync(dto, createdBy);
        return result.Success ? StatusCode(201, result) : BadRequest(result);
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _roleService.GetAllAsync();
        return Ok(result);
    }


    [HttpDelete("{roleName}")]
    public async Task<IActionResult> Delete(string roleName)
    {
        var deletedBy = User.FindFirstValue(ClaimTypes.Email) ?? "system";
        var result = await _roleService.DeleteAsync(roleName, deletedBy);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
