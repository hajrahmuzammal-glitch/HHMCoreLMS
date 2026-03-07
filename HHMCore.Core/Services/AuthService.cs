using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HHMCore.Core.DTOs.Auth;
using HHMCore.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using HHMCore.Core.Entities;

namespace HHMCore.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // Check if a user with this email already exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new Exception("A user with this email already exists.");

            // Create a new IdentityUser object (this is NOT saved yet)
            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName
            };

            // CreateAsync saves the user AND hashes the password automatically
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Registration failed: {errors}");
            }

            // Make sure the role exists before assigning it
            if (!await _roleManager.RoleExistsAsync(dto.Role))
                throw new Exception($"Role '{dto.Role}' does not exist.");

            // Assign the role to the user
            await _userManager.AddToRoleAsync(user, dto.Role);

            // Generate and return the JWT token
            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            // Find the user by email
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("Invalid email or password.");

            // CheckPasswordAsync compares the input against the stored hash
            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
                throw new Exception("Invalid email or password.");

            // Generate and return the JWT token
            return await GenerateAuthResponseAsync(user);
        }

        // Private helper — builds the JWT token and returns AuthResponseDto
        private async Task<AuthResponseDto> GenerateAuthResponseAsync(AppUser user)
        {
            // Get the roles assigned to this user
            var roles = await _userManager.GetRolesAsync(user);

            // Claims are pieces of information packed inside the token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? string.Empty)
            };

            // Read JWT settings from appsettings.json
            var key = _configuration["JWT:Key"]!;
            var issuer = _configuration["JWT:Issuer"]!;
            var audience = _configuration["JWT:Audience"]!;
            var duration = int.Parse(_configuration["JWT:DurationInMinutes"]!);

            // Create the signing key using our secret
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            // Build the token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(duration),
                signingCredentials: credentials
            );

            // Serialize the token object into the actual string (eyJ...)
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new AuthResponseDto
            {
                Token = tokenString,
                ExpiresAt = DateTime.UtcNow.AddMinutes(duration),
                FullName = user.FullName,
                Email = user.Email!,
                Role = roles.FirstOrDefault() ?? string.Empty
            };
        }
    }
}
