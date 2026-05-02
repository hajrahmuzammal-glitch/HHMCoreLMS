using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Role;
using HHMCore.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HHMCore.Core.Services;

    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<Entities.AppUser> _userManager;

        public RoleService(
            RoleManager<IdentityRole> roleManager,
            UserManager<Entities.AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<ApiResponse<RoleResponseDto>> CreateAsync(
            CreateRoleDto dto, string createdBy)
        {
            
            var normalizedName = dto.Name.Trim();

           
            var exists = await _roleManager.RoleExistsAsync(normalizedName);
            if (exists)
            {
                return ApiResponse<RoleResponseDto>.Fail($"Role '{normalizedName}' already exists.");
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(normalizedName));
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<RoleResponseDto>.Fail("Failed to create role.", errors);
            }

            var created = await _roleManager.FindByNameAsync(normalizedName);

            var response = new RoleResponseDto
            {
                Id = created!.Id,
                Name = created.Name!,
                UserCount = 0
            };

            return ApiResponse<RoleResponseDto>.Ok(response, $"Role '{normalizedName}' created successfully.");
        }

        public async Task<ApiResponse<List<RoleResponseDto>>> GetAllAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();

            var response = new List<RoleResponseDto>();

            foreach (var role in roles)
            {
                
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);

                response.Add(new RoleResponseDto
                {
                    Id = role.Id,
                    Name = role.Name!,
                    UserCount = usersInRole.Count
                });
            }

            return ApiResponse<List<RoleResponseDto>>.Ok(response, "Roles fetched.");
        }

        public async Task<ApiResponse> DeleteAsync(string roleName, string deletedBy)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return ApiResponse.Fail($"Role '{roleName}' not found.");
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            if (usersInRole.Count > 0)
            {
                return ApiResponse.Fail(
                    $"Cannot delete role '{roleName}'. {usersInRole.Count} user(s) are assigned to it. Reassign them first.");
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse.Fail("Failed to delete role.", errors);
            }

            return ApiResponse.Ok($"Role '{roleName}' deleted successfully.");
        }
    }
