using HRManager.WebAPI.DTOs;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IPermissionService
    {
        Task<List<PermissionDto>> GetAllPermissionsAsync();
        Task<List<PermissionDto>> GetPermissionsByModuleAsync(string module);
        Task<PermissionDto> GetPermissionAsync(Guid id);
        Task<PermissionDto> CreatePermissionAsync(CreatePermissionRequest request);
        Task<PermissionDto> UpdatePermissionAsync(Guid id, UpdatePermissionRequest request);
        Task<bool> TogglePermissionStatusAsync(Guid id);
    }
}
