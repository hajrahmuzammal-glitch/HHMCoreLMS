using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Student;
using HHMCore.Core.Interfaces;
using HHMCore.Core.Services;
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _studentService.GetByIdAsync(id);

            if (!result.Success || result.Data == null)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse.Fail("ID in URL does not match ID in request body."));

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
        [HttpGet("me")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _studentService.GetMeAsync(userId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        private string GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        private string GetCurrentUserEmail() =>
            User.FindFirstValue(ClaimTypes.Email) ?? "system";
    }
}