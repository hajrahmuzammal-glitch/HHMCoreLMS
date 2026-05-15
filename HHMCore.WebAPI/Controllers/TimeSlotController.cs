using System.Security.Claims;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.TimeSlot;
using HHMCore.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HHMCore.WebAPI.Controllers;

[ApiController]
[Route("api/timeslots")]
[Authorize]
public class TimeSlotController : ControllerBase
{
    private readonly ITimeSlotService _timeSlotService;

    public TimeSlotController(ITimeSlotService timeSlotService)
    {
        _timeSlotService = timeSlotService;
    }

    private string GetCurrentUserEmail() =>
        User.FindFirstValue(ClaimTypes.Email) ?? "system";

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateTimeSlotDto dto)
    {
        var result = await _timeSlotService.CreateAsync(dto, GetCurrentUserEmail());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _timeSlotService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _timeSlotService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTimeSlotDto dto)
    {
        var result = await _timeSlotService.UpdateAsync(id, dto, GetCurrentUserEmail());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _timeSlotService.DeleteAsync(id, GetCurrentUserEmail());
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
