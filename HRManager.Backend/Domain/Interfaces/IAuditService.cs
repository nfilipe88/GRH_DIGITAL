using HRManager.WebAPI.Domain.enums;
using HRManager.WebAPI.DTOs;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IAuditService
    {
        Task LogRoleChangeAsync(Guid roleId, AuditAction action, object? oldValues = null, object? newValues = null, string? changes = null);
        Task LogPermissionChangeAsync(Guid permissionId, AuditAction action, object? oldValues = null, object? newValues = null, string? changes = null);
        Task LogRolePermissionChangeAsync(Guid roleId, Guid permissionId, AuditAction action, string? changes = null);
        Task LogUserRoleChangeAsync(Guid userId, Guid roleId, AuditAction action, string? changes = null);
        Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(EntityType? entityType = null, Guid? entityId = null, int page = 1, int pageSize = 50);
    }
}
