namespace HHMCore.API.Controllers;

using System.Security.Claims;
using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Auth;
using HHMCore.Core.Entities;
using HHMCore.Core.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/account")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;

    public AccountController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Any logged-in user (Admin, Teacher, Student) can change their own password.
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        // Validate
        var validator = new ChangePasswordValidator();
        var validation = await validator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return BadRequest(ApiResponse.Fail(validation.Errors.Select(e => e.ErrorMessage).First()));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser is null)
        {
            return Unauthorized(ApiResponse.Fail("User not found."));
        }

        var result = await _userManager.ChangePasswordAsync(
            appUser, dto.CurrentPassword, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(ApiResponse.Fail("Password change failed.", errors));
        }

        return Ok(ApiResponse.Ok("Password changed successfully."));
    }

    /// <summary>
    /// Returns the identity-level info (email, name) of the current user.
    /// For role-specific profile (department, roll number etc.) use /api/students/me or /api/teachers/me
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser is null)
        {
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(appUser);

        return Ok(ApiResponse<object>.Ok(new
        {
            appUser.Id,
            appUser.FullName,
            appUser.Email,
            Roles = roles
        }, "Profile retrieved."));
    }
}