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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _teacherService.GetByIdAsync(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("department/{departmentId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByDepartment(Guid departmentId)
        {
            var result = await _teacherService.GetByDepartmentAsync(departmentId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeacherDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { success = false, message = "ID in URL does not match ID in body." });

            var existing = await _teacherService.GetByIdAsync(id);
            if (!existing.Success)
                return NotFound(existing);

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

        [HttpGet("me")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetMe()
        {
            var result = await _teacherService.GetMeAsync(GetCurrentUserId());
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPut("me/profile")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateTeacherProfileDto dto)
        {
            var userId = GetCurrentUserId();               
            if (string.IsNullOrEmpty(userId))             
                return Unauthorized();

            var result = await _teacherService.UpdateMyProfileAsync(userId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        private string GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        private string GetCurrentUserEmail() =>
            User.FindFirstValue(ClaimTypes.Email) ?? "system";

    }
}