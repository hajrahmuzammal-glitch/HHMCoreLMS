using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Auth;
using HHMCore.Core.Entities;
using HHMCore.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HHMCore.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;

        public AuthService(UserManager<AppUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto)
        {
            // Normalize email — "Admin@Test.COM" and "admin@test.com" must be treated as one
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
            if (existingUser != null)
                return ApiResponse<AuthResponseDto>.Fail("This email is already registered.");

            var appUser = new AppUser
            {
                FullName = dto.FullName.Trim(),
                Email = normalizedEmail,
                UserName = normalizedEmail
            };

            var createResult = await _userManager.CreateAsync(appUser, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                return ApiResponse<AuthResponseDto>.Fail("Registration failed.", errors);
            }

            // Try to assign the role — if the role does not exist in the DB, this returns a failure
            var roleResult = await _userManager.AddToRoleAsync(appUser, dto.Role);
            if (!roleResult.Succeeded)
            {
                // Clean up the Identity user so we do not leave an orphan account
                await _userManager.DeleteAsync(appUser);
                return ApiResponse<AuthResponseDto>.Fail(
                    $"Role '{dto.Role}' does not exist. An Admin must create it first.");
            }

            var token = await GenerateTokenAsync(appUser);
            var duration = int.Parse(_config["JWT:DurationInMinutes"]!);

            var response = new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(duration),
                FullName = appUser.FullName,
                Email = appUser.Email!,
                Role = dto.Role
            };

            return ApiResponse<AuthResponseDto>.Ok(response, "Registration successful.");
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            // Normalize before lookup — matches how we stored it
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            var user = await _userManager.FindByEmailAsync(normalizedEmail);

            // Check null separately — avoids the null reference warning on CheckPasswordAsync
            if (user is null)
                return ApiResponse<AuthResponseDto>.Fail("Invalid email or password.");

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
                return ApiResponse<AuthResponseDto>.Fail("Invalid email or password.");

            // GetRolesAsync is safe here — user is confirmed non-null above
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            var token = await GenerateTokenAsync(user);
            var duration = int.Parse(_config["JWT:DurationInMinutes"]!);

            var response = new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(duration),
                FullName = user.FullName,
                Email = user.Email!,
                Role = role
            };

            return ApiResponse<AuthResponseDto>.Ok(response, "Login successful.");
        }

        private async Task<string> GenerateTokenAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email,          user.Email!),
                new(ClaimTypes.Name,           user.FullName),
                new(ClaimTypes.Role,           role)
            };

            var keyBytes = Encoding.UTF8.GetBytes(_config["JWT:Key"]!);
            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var duration = int.Parse(_config["JWT:DurationInMinutes"]!);

            var token = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(duration),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}