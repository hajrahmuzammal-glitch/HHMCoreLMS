using System.Security.Claims;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Teacher;
using HHMCore.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HHMCore.WebAPI.Controllers;

[ApiController]
[Route("api/teachers")]
[Authorize]
public class TeacherController : ControllerBase
{
    private readonly ITeacherService _teacherService;

    public TeacherController(ITeacherService teacherService)
    {
        _teacherService = teacherService;
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateTeacherDto dto)
    {
        var result = await _teacherService.CreateAsync(dto, GetCurrentUserEmail());//by getting user email are we getting the user during the create method
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _teacherService.GetAllAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _teacherService.GetByIdAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("department/{departmentId:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> GetByDepartment(Guid departmentId)
    {
        var result = await _teacherService.GetByDepartmentAsync(departmentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeacherDto dto)
    {

        var existing = await _teacherService.GetByIdAsync(id);
        if (!existing.Success)
        {
            return NotFound(existing);
        }

        var result = await _teacherService.UpdateAsync(id, dto, GetCurrentUserEmail());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deletedBy = GetCurrentUserEmail();//i can just write GetCurrentUserEmail() 
        var result = await _teacherService.DeleteAsync(id, deletedBy);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("me")]
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<IActionResult> GetMe()
    {
        var result = await _teacherService.GetMeAsync(GetCurrentUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("me/profile")]
    [Authorize(Roles = AppRoles.Teacher)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateTeacherProfileDto dto)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _teacherService.UpdateMyProfileAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private string GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    private string GetCurrentUserEmail() =>
        User.FindFirstValue(ClaimTypes.Email) ?? "system";

}
