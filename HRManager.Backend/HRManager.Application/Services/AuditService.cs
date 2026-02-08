using HRManager.WebAPI.Domain.enums;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Helpers;
using HRManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace HRManager.WebAPI.Services
{
    public class AuditService : IAuditService
    {
        private readonly HRManagerDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditService> _logger;

        public AuditService(
            HRManagerDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuditService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task LogRoleChangeAsync(Guid roleId, AuditAction action, object? oldValues = null, object? newValues = null, string? changes = null)
        {
            await LogAsync(EntityType.Role, roleId, action, oldValues, newValues, changes);
        }

        public async Task LogPermissionChangeAsync(Guid permissionId, AuditAction action, object? oldValues = null, object? newValues = null, string? changes = null)
        {
            await LogAsync(EntityType.Permission, permissionId, action, oldValues, newValues, changes);
        }

        public async Task LogRolePermissionChangeAsync(Guid roleId, Guid permissionId, AuditAction action, string? changes = null)
        {
            // Criar um ID composto para a relação
            var compositeId = $"{roleId}:{permissionId}";
            var entityId = GuidUtility.Create(GuidUtility.IsoOidNamespace, compositeId);

            await LogAsync(EntityType.RolePermission, entityId, action, null, null, changes);
        }

        public async Task LogUserRoleChangeAsync(Guid userId, Guid roleId, AuditAction action, string? changes = null)
        {
            var compositeId = $"{userId}:{roleId}";
            var entityId = GuidUtility.Create(GuidUtility.IsoOidNamespace, compositeId);

            await LogAsync(EntityType.UserRole, entityId, action, null, null, changes);
        }

        private async Task LogAsync(EntityType entityType, Guid entityId, AuditAction action, object? oldValues, object? newValues, string? changes)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty) return;

                var log = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityType = entityType,
                    EntityId = entityId,
                    Action = action,
                    OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                    NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                    Changes = changes,
                    UserId = userId,
                    UserIp = GetUserIp(),
                    UserAgent = GetUserAgent(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar log de auditoria");
            }
        }

        public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(EntityType? entityType = null, Guid? entityId = null, int page = 1, int pageSize = 50)
        {
            var query = _context.AuditLogs
                .Include(al => al.User)
                .AsQueryable();

            if (entityType.HasValue)
                query = query.Where(al => al.EntityType == entityType.Value);

            if (entityId.HasValue)
                query = query.Where(al => al.EntityId == entityId.Value);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(al => al.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(al => new AuditLogDto
                {
                    Id = al.Id,
                    EntityType = al.EntityType.ToString(),
                    EntityId = al.EntityId,
                    Action = al.Action.ToString(),
                    Changes = al.Changes,
                    UserName = al.User.NomeCompleto,
                    UserEmail = al.User.Email ?? "",
                    CreatedAt = al.CreatedAt,
                    OldValues = !string.IsNullOrEmpty(al.OldValues)
                        ? JsonSerializer.Deserialize<Dictionary<string, object>>(al.OldValues, new JsonSerializerOptions())
                        : null,
                    NewValues = !string.IsNullOrEmpty(al.NewValues)
                        ? JsonSerializer.Deserialize<Dictionary<string, object>>(al.NewValues, new JsonSerializerOptions())
                        : null
                })
                .ToListAsync();

            return new PagedResult<AuditLogDto>
            {
                Items = logs,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }
        private Guid GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        private string GetUserIp()
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private string GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString() ?? "Unknown";
        }
    }
}
