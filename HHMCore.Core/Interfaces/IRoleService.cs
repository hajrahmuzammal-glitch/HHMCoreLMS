using HHMCore.Core.Common;
using HHMCore.Core.DTOs.Role;
using HHMCore.Core.DTOs.Room;

namespace HHMCore.Core.Interfaces;

public interface IRoleService
{
    // Create a new role — Admin only, audited
    Task<ApiResponse<RoleResponseDto>> CreateAsync(CreateRoleDto dto, string createdBy);

    // Get all roles in the system
    Task<ApiResponse<List<RoleResponseDto>>> GetAllAsync();

    // Delete a role — blocked if any users are assigned to it
    Task<ApiResponse> DeleteAsync(string roleName, string deletedBy);
}