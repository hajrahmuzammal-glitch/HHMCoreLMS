using HHMCore.Core.DTOs.Teacher;
using HHMCore.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HHMCore.WebAPI.Controllers
{
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateTeacherDto dto)
        {
            var result = await _teacherService.CreateAsync(dto, GetCurrentUserEmail());
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _teacherService.GetAllAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _teacherService.GetByIdAsync(id);

            if (!result.Success)
                return NotFound(result);

            if (IsTeacher() && result.Data != null && result.Data.UserId != GetCurrentUserId())
                return StatusCode(403, new { success = false, message = "You are not authorized to view this profile." });

            return Ok(result);
        }

        [HttpGet("department/{departmentId:guid}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetByDepartment(Guid departmentId)
        {
            var result = await _teacherService.GetByDepartmentAsync(departmentId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeacherDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { success = false, message = "ID in URL does not match ID in body." });

            var existing = await _teacherService.GetByIdAsync(id);
            if (!existing.Success)
                return NotFound(existing);

            if (IsTeacher() && existing.Data != null && existing.Data.UserId != GetCurrentUserId())
                return StatusCode(403, new { success = false, message = "You are not authorized to update this profile." });

            var result = await _teacherService.UpdateAsync(dto, GetCurrentUserEmail());
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _teacherService.DeleteAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        private string GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        private string GetCurrentUserEmail() =>
            User.FindFirstValue(ClaimTypes.Email) ?? "system";

        private bool IsTeacher() =>
            User.FindFirstValue(ClaimTypes.Role) == "Teacher";
    }
}