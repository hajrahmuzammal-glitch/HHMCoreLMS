using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Student;
using HHMCore.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HHMCore.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
        {
            var result = await _studentService.CreateAsync(dto, GetCurrentUserEmail());
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _studentService.GetAllAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _studentService.GetByIdAsync(id);

            if (!result.Success || result.Data == null)
                return NotFound(result);

            if (IsStudent() && result.Data.UserId != GetCurrentUserId())
                return StatusCode(403, ApiResponse.Fail("You are not authorized to access this profile."));

            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse.Fail("ID in URL does not match ID in request body."));

            if (IsStudent())
            {
                var existing = await _studentService.GetByIdAsync(id);
                if (!existing.Success || existing.Data == null)
                    return NotFound(existing);

                if (existing.Data.UserId != GetCurrentUserId())
                    return StatusCode(403, ApiResponse.Fail("You are not authorized to update this profile."));
            }

            var result = await _studentService.UpdateAsync(dto, GetCurrentUserEmail());
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _studentService.DeleteAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        private string GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        private string GetCurrentUserEmail() =>
            User.FindFirstValue(ClaimTypes.Email) ?? "system";

        private bool IsStudent() =>
            User.FindFirstValue(ClaimTypes.Role) == "Student";
    }
}