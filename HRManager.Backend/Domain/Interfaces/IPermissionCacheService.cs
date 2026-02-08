using HRManager.WebAPI.DTOs;

namespace HRManager.WebAPI.Infrastructure.Caching
{
    public interface IPermissionCacheService
    {
        Task<List<PermissionDto>> GetUserPermissionsAsync(Guid userId);
        Task<List<RoleDetailDto>> GetRoleWithPermissionsAsync(Guid roleId);
        Task SetUserPermissionsAsync(Guid userId, List<PermissionDto> permissions);
        Task SetRoleWithPermissionsAsync(Guid roleId, RoleDetailDto role);
        Task RemoveUserPermissionsAsync(Guid userId);
        Task RemoveAllPermissionsCacheAsync();
        Task RemoveRolePermissionsCacheAsync(Guid roleId);
        Task ClearCacheAsync(Guid userId);
    }
}
