using HRManager.WebAPI.DTOs;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IRoleService
    {
        Task<PagedResult<RoleDto>> GetRolesAsync(int page = 1, int pageSize = 20);
        Task<RoleDetailDto> GetRoleByIdAsync(Guid id);
        Task<RoleDetailDto> CreateRoleAsync(CreateRoleRequest request);
        Task<RoleDetailDto> UpdateRoleAsync(Guid id, UpdateRoleRequest request);
        Task<bool> DeleteRoleAsync(Guid id);
        Task<bool> AssignPermissionsToRoleAsync(Guid roleId, List<Guid> permissionIds);
        Task<bool> RemovePermissionsFromRoleAsync(Guid roleId, List<Guid> permissionIds);
        Task<List<PermissionDto>> GetRolePermissionsAsync(Guid roleId);
    }
}
