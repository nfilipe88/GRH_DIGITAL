using HRManager.WebAPI.Domain.enums;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace HRManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [PermissionAuthorize("ROLES_IMPORT_EXPORT")]
    public class RoleImportExportController : ControllerBase
    {
        private readonly IRoleImportExportService _importExportService;
        private readonly ILogger<RoleImportExportController> _logger;

        public RoleImportExportController(
            IRoleImportExportService importExportService,
            ILogger<RoleImportExportController> logger)
        {
            _importExportService = importExportService;
            _logger = logger;
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportRoles([FromBody] ExportRolesRequest request)
        {
            try
            {
                var fileBytes = await _importExportService.ExportRolesAsync(request.RoleIds, request.IncludeSystemRoles);

                return File(fileBytes, "application/json",
                    $"hr_roles_export_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao exportar roles");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("export/all")]
        public async Task<IActionResult> ExportAllRoles()
        {
            try
            {
                var fileBytes = await _importExportService.ExportAllRolesAsync();

                return File(fileBytes, "application/json",
                    $"hr_all_roles_export_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao exportar todas as roles");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("import")]
        [RequestSizeLimit(10_485_760)] // 10MB
        public async Task<ActionResult<RoleImportResult>> ImportRoles([FromBody] RoleImportRequest request)
        {
            try
            {
                var result = await _importExportService.ImportRolesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao importar roles");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("import/file")]
        [RequestSizeLimit(10_485_760)] // 10MB
        public async Task<ActionResult<RoleImportResult>> ImportRolesFromFile(IFormFile file, [FromQuery] ImportStrategy strategy = ImportStrategy.Merge)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Arquivo não enviado" });

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var jsonContent = await reader.ReadToEndAsync();

                var result = await _importExportService.ImportRolesFromJsonAsync(jsonContent, strategy);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao importar roles de arquivo");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{roleId}/export-template")]
        public async Task<ActionResult<RoleTemplateDto>> ExportAsTemplate(Guid roleId, [FromQuery] string templateName)
        {
            try
            {
                var template = await _importExportService.ExportRoleAsTemplateAsync(roleId, templateName);
                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao exportar role como template");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
