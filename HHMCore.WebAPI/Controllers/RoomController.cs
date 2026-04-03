using HHMCore.Core.DTOs.Room;
using HHMCore.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HHMCore.WebAPI.Controllers;

[ApiController]
[Route("api/rooms")]
[Authorize]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    private string GetCurrentUserEmail() =>
        User.FindFirstValue(ClaimTypes.Email) ?? "system";

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
    {
        var result = await _roomService.CreateAsync(dto, GetCurrentUserEmail());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBulk([FromBody] List<CreateRoomDto> dtos)
    {
        var result = await _roomService.CreateBulkAsync(dtos, GetCurrentUserEmail());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _roomService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _roomService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("available")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAvailable([FromQuery] Guid timeSlotId, [FromQuery] Guid semesterId)
    {
        var result = await _roomService.GetAvailableRoomsAsync(timeSlotId, semesterId);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoomDto dto)
    {
        var result = await _roomService.UpdateAsync(id, dto, GetCurrentUserEmail());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _roomService.DeleteAsync(id, GetCurrentUserEmail());
        return result.Success ? Ok(result) : BadRequest(result);
    }
}