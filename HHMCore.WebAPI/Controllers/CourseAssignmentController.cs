using HHMCore.Core.DTOs.CourseAssignment;
using HHMCore.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HHMCore.WebAPI.Controllers;

[ApiController]
[Route("api/course-assignments")]
[Authorize]
public class CourseAssignmentController : ControllerBase
{
    private readonly ICourseAssignmentService _courseAssignmentService;

    public CourseAssignmentController(ICourseAssignmentService courseAssignmentService)
    {
        _courseAssignmentService = courseAssignmentService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCourseAssignmentDto dto)
    {
        var result = await _courseAssignmentService.CreateAsync(dto, GetCurrentUserEmail());
        return result.Success ? StatusCode(201, result) : BadRequest(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _courseAssignmentService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _courseAssignmentService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("semester/{semesterId:guid}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetBySemester(Guid semesterId)
    {
        var result = await _courseAssignmentService.GetBySemesterAsync(semesterId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("teacher/{teacherId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetByTeacher(Guid teacherId)
    {
        var result = await _courseAssignmentService.GetByTeacherAsync(teacherId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetMyAssignments()
    {
        var result = await _courseAssignmentService.GetMyAssignmentsAsync(GetCurrentUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseAssignmentDto dto)
    {
        var result = await _courseAssignmentService.UpdateAsync(id, dto, GetCurrentUserEmail());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _courseAssignmentService.DeleteAsync(id, GetCurrentUserEmail());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private string GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    private string GetCurrentUserEmail() =>
        User.FindFirstValue(ClaimTypes.Email) ?? "system";
}