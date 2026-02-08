using HRManager.WebAPI.Helpers;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IPermissionValidationService
    {
        Task<ValidationResult> ValidateRolePermissionsAsync(Guid roleId, List<Guid> permissionIds);
        Task<ValidationResult> ValidateUserPermissionsAsync(Guid userId, List<Guid> roleIds);
        Task<List<PermissionDependency>> GetPermissionDependenciesAsync(string permissionCode);
        Task<List<PermissionConflict>> GetPermissionConflictsAsync(List<string> permissionCodes);
        Task<PermissionCompatibilityReport> CheckCompatibilityAsync(List<string> permissionCodes);
    }
}
