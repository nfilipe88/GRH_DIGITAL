using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Helpers;
using Microsoft.AspNetCore.Mvc;
using HRManager.WebAPI.Infrastructure;

namespace HRManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [PermissionAuthorize("PERMISSIONS_VALIDATE")]
    public class PermissionValidationController : ControllerBase
    {
        private readonly IPermissionValidationService _validationService;
        private readonly ILogger<PermissionValidationController> _logger;

        public PermissionValidationController(
            IPermissionValidationService validationService,
            ILogger<PermissionValidationController> logger)
        {
            _validationService = validationService;
            _logger = logger;
        }

        [HttpPost("validate/role")]
        public async Task<ActionResult<ValidationResult>> ValidateRolePermissions([FromBody] ValidateRoleRequest request)
        {
            try
            {
                var result = await _validationService.ValidateRolePermissionsAsync(request.RoleId, request.PermissionIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar permissões da role");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("validate/compatibility")]
        public async Task<ActionResult<PermissionCompatibilityReport>> CheckCompatibility([FromBody] CheckCompatibilityRequest request)
        {
            try
            {
                var report = await _validationService.CheckCompatibilityAsync(request.PermissionCodes);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar compatibilidade de permissões");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("dependencies/{permissionCode}")]
        public async Task<ActionResult<List<PermissionDependency>>> GetDependencies(string permissionCode)
        {
            try
            {
                var dependencies = await _validationService.GetPermissionDependenciesAsync(permissionCode);
                return Ok(dependencies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter dependências da permissão");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("conflicts")]
        public async Task<ActionResult<List<PermissionConflict>>> GetConflicts([FromBody] List<string> permissionCodes)
        {
            try
            {
                var conflicts = await _validationService.GetPermissionConflictsAsync(permissionCodes);
                return Ok(conflicts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter conflitos de permissões");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
