using HRManager.WebAPI.Domain.enums;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace HRManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [PermissionAuthorize("AUDIT_VIEW")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditLogsController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAuditLogs(
            [FromQuery] EntityType? entityType,
            [FromQuery] Guid? entityId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var logs = await _auditService.GetAuditLogsAsync(entityType, entityId, page, pageSize);
            return Ok(logs);
        }

        [HttpGet("roles/{roleId}")]
        public async Task<ActionResult<PagedResult<AuditLogDto>>> GetRoleAuditLogs(
            Guid roleId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var logs = await _auditService.GetAuditLogsAsync(EntityType.Role, roleId, page, pageSize);
            return Ok(logs);
        }

        [HttpGet("permissions/{permissionId}")]
        public async Task<ActionResult<PagedResult<AuditLogDto>>> GetPermissionAuditLogs(
            Guid permissionId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var logs = await _auditService.GetAuditLogsAsync(EntityType.Permission, permissionId, page, pageSize);
            return Ok(logs);
        }
    }
}
