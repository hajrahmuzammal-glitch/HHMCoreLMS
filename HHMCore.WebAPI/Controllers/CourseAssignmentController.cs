
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
    private readonly ICourseAssignmentService _service;
    private readonly IUnitOfWork _unitOfWork;

    public CourseAssignmentController(
        ICourseAssignmentService service,
        IUnitOfWork unitOfWork)
    {
        _service = service;
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCourseAssignmentDto dto)
    {
        var result = await _service.CreateAsync(dto, GetCurrentUserEmail());
        return result.Success ? StatusCode(201, result) : BadRequest(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("semester/{semesterId:guid}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetBySemester(Guid semesterId)
    {
        var result = await _service.GetBySemesterAsync(semesterId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // Teacher sees own schedule — blocked from viewing other teachers
    [HttpGet("teacher/{teacherId:guid}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetByTeacher(Guid teacherId)
    {
        if (IsTeacher())
        {
            var currentUserId = GetCurrentUserId();
            var teacher = await _unitOfWork.Teachers.FindOneAsync(
                t => t.Id == teacherId && t.UserId == currentUserId);

            if (teacher == null)
                return StatusCode(403, new
                {
                    success = false,
                    message = "You are not authorized to view this schedule."
                });
        }

        var result = await _service.GetByTeacherAsync(teacherId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseAssignmentDto dto)
    {
        dto.Id = id;
        var result = await _service.UpdateAsync(dto, GetCurrentUserEmail());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private string GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    private string GetCurrentUserEmail() =>
        User.FindFirstValue(ClaimTypes.Email) ?? "system";

    private bool IsTeacher() =>
        User.FindFirstValue(ClaimTypes.Role) == "Teacher";
}