using HRManager.WebAPI.Domain.enums;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace HRManager.WebAPI.Services
{
    public class RoleImportExportService : IRoleImportExportService
    {
        private readonly HRManagerDbContext _context;
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;
        private readonly IAuditService _auditService;
        private readonly ILogger<RoleImportExportService> _logger;

        public RoleImportExportService(
            HRManagerDbContext context,
            IRoleService roleService,
            IPermissionService permissionService,
            IAuditService auditService,
            ILogger<RoleImportExportService> logger)
        {
            _context = context;
            _roleService = roleService;
            _permissionService = permissionService;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<byte[]> ExportRolesAsync(List<Guid> roleIds, bool includeSystemRoles = false)
        {
            var query = _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .Where(r => roleIds.Contains(r.Id));

            if (!includeSystemRoles)
                query = query.Where(r => !r.IsSystemRole);

            var roles = await query.ToListAsync();
            //var roles = JsonSerializer.Deserialize<List<RoleExportDto>>(jsonContent) ?? new List<RoleExportDto>();

            var exportDtos = new List<RoleExportDto>();
            foreach (var role in roles)
            {
                var exportDto = new RoleExportDto
                {
                    Name = role.Name ?? string.Empty,
                    Description = role.Description ?? string.Empty,
                    IsSystemRole = role.IsSystemRole,
                    IsActive = role.IsActive,
                    PermissionCodes = role.RolePermissions
                        .Select(rp => rp.Permission.Code)
                        .ToList(),
                    Metadata = new Dictionary<string, object>
                    {
                        { "OriginalId", role.Id },
                        { "CreatedAt", role.CreatedAt },
                        { "InstituicaoId", role.InstituicaoId?.ToString() ?? "Global" }
                    }
                };

                exportDtos.Add(exportDto);
            }

            var exportPackage = new
            {
                Roles = exportDtos,
                ExportInfo = new
                {
                    ExportDate = DateTime.UtcNow,
                    Version = "1.0",
                    TotalRoles = exportDtos.Count,
                    System = "HR Manager"
                }
            };

            var json = JsonSerializer.Serialize(exportPackage, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return Encoding.UTF8.GetBytes(json);
        }

        public async Task<byte[]> ExportAllRolesAsync()
        {
            var roleIds = await _context.Roles
                .Where(r => !r.IsSystemRole) // Não exportar roles do sistema
                .Select(r => r.Id)
                .ToListAsync();

            return await ExportRolesAsync(roleIds, false);
        }

        public async Task<RoleImportResult> ImportRolesAsync(RoleImportRequest request, Guid? instituicaoId = null)
        {
            var result = new RoleImportResult();
            var currentUserId = GetCurrentUserId();

            // Obter todas as permissões para mapeamento
            var allPermissions = await _permissionService.GetAllPermissionsAsync();
            var permissionCodeMap = allPermissions.ToDictionary(p => p.Code, p => p.Id);

            foreach (var roleDto in request.Roles)
            {
                var detail = new RoleImportDetail { RoleName = roleDto.Name };

                try
                {
                    // Validar role
                    var validationResult = ValidateRoleForImport(roleDto, permissionCodeMap);
                    if (!validationResult.IsValid)
                    {
                        detail.Status = ImportStatus.Error;
                        detail.Errors = validationResult.Errors;
                        result.Failed++;
                        result.Details.Add(detail);
                        continue;
                    }

                    if (validationResult.Warnings.Any())
                        detail.Warnings = validationResult.Warnings;

                    // Verificar se role já existe
                    var existingRole = await _context.Roles
                        .FirstOrDefaultAsync(r => r.Name == roleDto.Name &&
                                                 r.InstituicaoId == instituicaoId);

                    var permissionIds = roleDto.PermissionCodes
                        .Where(code => permissionCodeMap.ContainsKey(code))
                        .Select(code => permissionCodeMap[code])
                        .ToList();

                    if (existingRole != null)
                    {
                        // Role existe - decidir o que fazer baseado na estratégia
                        switch (request.Strategy)
                        {
                            case ImportStrategy.Merge:
                                await UpdateRoleAsync(existingRole, roleDto, permissionIds);
                                detail.Status = ImportStatus.Success;
                                detail.Message = "Role atualizada (merge)";
                                result.Updated++;
                                break;

                            case ImportStrategy.Replace:
                                await ReplaceRoleAsync(existingRole, roleDto, permissionIds);
                                detail.Status = ImportStatus.Success;
                                detail.Message = "Role substituída";
                                result.Updated++;
                                break;

                            case ImportStrategy.Update:
                                if (request.OverwriteExisting)
                                {
                                    await UpdateRoleAsync(existingRole, roleDto, permissionIds);
                                    detail.Status = ImportStatus.Success;
                                    detail.Message = "Role atualizada";
                                    result.Updated++;
                                }
                                else
                                {
                                    detail.Status = ImportStatus.Skipped;
                                    detail.Message = "Role já existe (skip)";
                                    result.Skipped++;
                                }
                                break;

                            default:
                                detail.Status = ImportStatus.Skipped;
                                detail.Message = "Estratégia não suportada";
                                result.Skipped++;
                                break;
                        }
                    }
                    else
                    {
                        // Criar nova role
                        await CreateRoleAsync(roleDto, permissionIds, instituicaoId);
                        detail.Status = ImportStatus.Success;
                        detail.Message = "Role criada";
                        result.Created++;
                    }

                    // Registrar log de auditoria
                    await _auditService.LogRoleChangeAsync(
                        existingRole?.Id ?? Guid.Empty,
                        existingRole != null ? AuditAction.Updated : AuditAction.Created,
                        existingRole != null ? new { existingRole.Name, existingRole.Description } : null,
                        new { roleDto.Name, roleDto.Description },
                        $"Role importada via sistema. Estratégia: {request.Strategy}"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao importar role {RoleName}", roleDto.Name);
                    detail.Status = ImportStatus.Error;
                    detail.Errors = new List<string> { ex.Message };
                    result.Failed++;
                }

                result.Details.Add(detail);
                result.TotalProcessed++;
            }

            return result;
        }

        public async Task<RoleImportResult> ImportRolesFromJsonAsync(string jsonContent, ImportStrategy strategy = ImportStrategy.Merge)
        {
            try
            {
                var exportPackage = JsonSerializer.Deserialize<dynamic>(jsonContent);
                var roles = JsonSerializer.Deserialize<List<RoleExportDto>>(exportPackage?.RootElement.GetProperty("roles").ToString() ?? "[]");

                var request = new RoleImportRequest
                {
                    Roles = roles ?? new List<RoleExportDto>(),
                    Strategy = strategy,
                    OverwriteExisting = true
                };

                return await ImportRolesAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao importar roles de JSON");
                throw;
            }
        }

        public async Task<RoleTemplateDto> ExportRoleAsTemplateAsync(Guid roleId, string templateName)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
                throw new KeyNotFoundException("Role não encontrada");

            var permissionCodes = role.RolePermissions
                .Select(rp => rp.Permission.Code)
                .ToList();

            return new RoleTemplateDto
            {
                Id = Guid.NewGuid(),
                Name = templateName,
                DisplayName = $"{role.Name} Template",
                Description = $"Template baseado na role '{role.Name}'",
                IsSystemTemplate = false,
                PermissionCodes = permissionCodes,
                Category = "Custom",
                Tags = new List<string> { "Importado", "Custom" },
                CreatedAt = DateTime.UtcNow
            };
        }

        private async Task CreateRoleAsync(RoleExportDto dto, List<Guid> permissionIds, Guid? instituicaoId)
        {
            var request = new CreateRoleRequest
            {
                Name = dto.Name,
                Description = dto.Description,
                PermissionIds = permissionIds
            };

            await _roleService.CreateRoleAsync(request);
        }

        private async Task UpdateRoleAsync(Role existingRole, RoleExportDto dto, List<Guid> permissionIds)
        {
            // Atualizar permissões
            await _roleService.AssignPermissionsToRoleAsync(existingRole.Id, permissionIds);

            // Atualizar outros campos se necessário
            if (!string.IsNullOrEmpty(dto.Description) && existingRole.Description != dto.Description)
            {
                existingRole.Description = dto.Description;
                await _context.SaveChangesAsync();
            }
        }

        private async Task ReplaceRoleAsync(Role existingRole, RoleExportDto dto, List<Guid> permissionIds)
        {
            // Remover todas as permissões existentes
            var existingPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == existingRole.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            if (existingPermissions.Any())
            {
                await _roleService.RemovePermissionsFromRoleAsync(existingRole.Id, existingPermissions);
            }

            // Atribuir novas permissões
            await _roleService.AssignPermissionsToRoleAsync(existingRole.Id, permissionIds);

            // Atualizar outros campos
            existingRole.Description = dto.Description;
            existingRole.IsActive = dto.IsActive;
            await _context.SaveChangesAsync();
        }

        private (bool IsValid, List<string> Errors, List<string> Warnings) ValidateRoleForImport(
            RoleExportDto dto, Dictionary<string, Guid> permissionCodeMap)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Name))
                errors.Add("Nome da role é obrigatório");

            if (dto.Name.Length > 100)
                errors.Add("Nome da role não pode exceder 100 caracteres");

            // Verificar permissões não encontradas
            var missingPermissions = dto.PermissionCodes
                .Where(code => !permissionCodeMap.ContainsKey(code))
                .ToList();

            if (missingPermissions.Any())
            {
                warnings.Add($"Permissões não encontradas: {string.Join(", ", missingPermissions)}");
            }

            if (dto.PermissionCodes.Count == 0)
                warnings.Add("Role não tem permissões atribuídas");

            return (errors.Count == 0, errors, warnings);
        }

        private Guid GetCurrentUserId()
        {
            // Implementar lógica para obter ID do usuário atual
            return Guid.NewGuid(); // Placeholder
        }
    }
}
