using FluentValidation;
using HRManager.WebAPI.Domain.enums;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Services
{
    public class RoleTemplateService : IRoleTemplateService
    {
        private readonly HRManagerDbContext _context;
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleTemplateService> _logger;

        public RoleTemplateService(
            HRManagerDbContext context,
            IRoleService roleService,
            ILogger<RoleTemplateService> logger)
        {
            _context = context;
            _roleService = roleService;
            _logger = logger;
        }

        public async Task<List<RoleTemplateDto>> GetSystemTemplatesAsync()
        {
            // Templates embutidos no sistema
            var systemTemplates = new List<RoleTemplateDto>
            {
                new RoleTemplateDto
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    Name = "AdministradorRH",
                    DisplayName = "Administrador de RH",
                    Description = "Acesso completo ao módulo de Recursos Humanos",
                    IsSystemTemplate = true,
                    PermissionCodes = new List<string>
                    {
                        "USERS_VIEW", "USERS_CREATE", "USERS_EDIT", "USERS_DELETE", "USERS_CHANGE_ROLE",
                        "ROLES_VIEW", "ROLES_CREATE", "ROLES_EDIT", "ROLES_DELETE", "ROLES_MANAGE_PERMISSIONS",
                        "ABSENCES_VIEW_ALL", "ABSENCES_APPROVE", "ABSENCES_MANAGE",
                        "INSTITUTIONS_VIEW", "INSTITUTIONS_EDIT",
                        "PERMISSIONS_VIEW", "PERMISSIONS_MANAGE",
                        "AUDIT_VIEW"
                    },
                    Category = "Administração",
                    Tags = new List<string> { "RH", "Administrador", "Completo" }
                },
                new RoleTemplateDto
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    Name = "GestorEquipa",
                    DisplayName = "Gestor de Equipa",
                    Description = "Pode gerir a sua equipa e aprovar ausências",
                    IsSystemTemplate = true,
                    PermissionCodes = new List<string>
                    {
                        "USERS_VIEW",
                        "ABSENCES_VIEW_TEAM", "ABSENCES_APPROVE",
                        "EVALUATIONS_VIEW_TEAM", "EVALUATIONS_CREATE"
                    },
                    Category = "Gestão",
                    Tags = new List<string> { "Gestor", "Equipa", "Aprovações" }
                },
                new RoleTemplateDto
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    Name = "Colaborador",
                    DisplayName = "Colaborador",
                    Description = "Acesso básico do sistema para colaboradores",
                    IsSystemTemplate = true,
                    PermissionCodes = new List<string>
                    {
                        "ABSENCES_VIEW_OWN", "ABSENCES_CREATE",
                        "EVALUATIONS_VIEW_OWN",
                        "PROFILE_VIEW", "PROFILE_EDIT"
                    },
                    Category = "Colaborador",
                    Tags = new List<string> { "Básico", "Colaborador" }
                },
                new RoleTemplateDto
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                    Name = "Relatorios",
                    DisplayName = "Analista de Relatórios",
                    Description = "Acesso a relatórios e análises",
                    IsSystemTemplate = true,
                    PermissionCodes = new List<string>
                    {
                        "REPORTS_VIEW", "REPORTS_GENERATE", "REPORTS_EXPORT",
                        "DASHBOARD_VIEW", "ANALYTICS_VIEW"
                    },
                    Category = "Análise",
                    Tags = new List<string> { "Relatórios", "Análise", "Dashboard" }
                }
            };

            return await Task.FromResult(systemTemplates);
        }

        public async Task<List<RoleTemplateDto>> GetUserTemplatesAsync(Guid userId)
        {
            var userTemplates = await _context.RoleTemplates
                .Where(rt => rt.CreatedBy == userId && rt.IsActive)
                .Select(rt => new RoleTemplateDto
                {
                    Id = rt.Id,
                    Name = rt.Name,
                    DisplayName = rt.DisplayName,
                    Description = rt.Description,
                    IsSystemTemplate = false,
                    PermissionCodes = rt.PermissionCodes,
                    Category = rt.Category,
                    Tags = rt.Tags,
                    UsageCount = rt.UsageCount,
                    LastUsed = rt.LastUsed,
                    CreatedAt = rt.CreatedAt
                })
                .ToListAsync();

            var systemTemplates = await GetSystemTemplatesAsync();
            return systemTemplates.Concat(userTemplates).ToList();
        }

        public async Task<RoleTemplateDto> CreateTemplateAsync(CreateTemplateRequest request)
        {
            // Validar se template com mesmo nome já existe
            var existingTemplate = await _context.RoleTemplates
                .FirstOrDefaultAsync(rt => rt.Name == request.Name && rt.CreatedBy == request.CreatedBy);

            if (existingTemplate != null)
                throw new ValidationException($"Já existe um template com o nome '{request.Name}'");

            var template = new RoleTemplate
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                DisplayName = request.DisplayName,
                Description = request.Description,
                PermissionCodes = request.PermissionCodes,
                Category = request.Category,
                Tags = request.Tags,
                CreatedBy = request.CreatedBy,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.RoleTemplates.Add(template);
            await _context.SaveChangesAsync();

            return MapToDto(template);
        }

        public async Task<RoleDetailDto> CreateRoleFromTemplateAsync(Guid templateId, string roleName, string? description = null)
        {
            var templates = await GetSystemTemplatesAsync();
            var template = templates.FirstOrDefault(t => t.Id == templateId);

            if (template == null)
                throw new KeyNotFoundException("Template não encontrado");

            // Obter IDs das permissões
            var permissionIds = await _context.Permissions
                .Where(p => template.PermissionCodes.Contains(p.Code))
                .Select(p => p.Id)
                .ToListAsync();

            // Criar nova role
            var createRequest = new CreateRoleRequest
            {
                Name = roleName,
                Description = description ?? template.Description,
                PermissionIds = permissionIds
            };

            return await _roleService.CreateRoleAsync(createRequest);
        }

        public async Task<RoleDetailDto> ApplyTemplateToRoleAsync(Guid roleId, Guid templateId)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
                throw new KeyNotFoundException("Role não encontrada");

            // Obter template
            RoleTemplateDto template;
            if (templateId.ToString().StartsWith("00000000-0000-0000-0000"))
            {
                // Template do sistema
                var systemTemplates = await GetSystemTemplatesAsync();
                template = systemTemplates.FirstOrDefault(t => t.Id == templateId)
                    ?? throw new KeyNotFoundException("Template do sistema não encontrado");
            }
            else
            {
                // Template do usuário
                var userTemplate = await _context.RoleTemplates.FindAsync(templateId)
                    ?? throw new KeyNotFoundException("Template não encontrado");
                template = MapToDto(userTemplate);
            }

            // Obter IDs das permissões do template
            var permissionIds = await _context.Permissions
                .Where(p => template.PermissionCodes.Contains(p.Code))
                .Select(p => p.Id)
                .ToListAsync();

            // Aplicar permissões à role
            await _roleService.AssignPermissionsToRoleAsync(roleId, permissionIds);

            // Atualizar contador de uso do template
            await UpdateTemplateUsageAsync(templateId);

            return await _roleService.GetRoleByIdAsync(roleId);
        }

        public async Task<List<PermissionComparison>> CompareRoleWithTemplateAsync(Guid roleId, Guid templateId)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
                throw new KeyNotFoundException("Role não encontrada");

            // Obter template
            RoleTemplateDto template;
            if (templateId.ToString().StartsWith("00000000-0000-0000-0000"))
            {
                var systemTemplates = await GetSystemTemplatesAsync();
                template = systemTemplates.FirstOrDefault(t => t.Id == templateId)
                    ?? throw new KeyNotFoundException("Template não encontrado");
            }
            else
            {
                var userTemplate = await _context.RoleTemplates.FindAsync(templateId)
                    ?? throw new KeyNotFoundException("Template não encontrado");
                template = MapToDto(userTemplate);
            }

            var rolePermissionCodes = role.RolePermissions
                .Select(rp => rp.Permission.Code)
                .ToList();

            var templatePermissionCodes = template.PermissionCodes;

            var comparisons = new List<PermissionComparison>();

            // Permissões que estão no template mas não na role
            var missingPermissions = templatePermissionCodes
                .Except(rolePermissionCodes)
                .ToList();

            foreach (var code in missingPermissions)
            {
                comparisons.Add(new PermissionComparison
                {
                    PermissionCode = code,
                    Status = ComparisonStatus.Missing,
                    InTemplate = true,
                    InRole = false
                });
            }

            // Permissões que estão na role mas não no template
            var extraPermissions = rolePermissionCodes
                .Except(templatePermissionCodes)
                .ToList();

            foreach (var code in extraPermissions)
            {
                comparisons.Add(new PermissionComparison
                {
                    PermissionCode = code,
                    Status = ComparisonStatus.Extra,
                    InTemplate = false,
                    InRole = true
                });
            }

            // Permissões comuns
            var commonPermissions = rolePermissionCodes
                .Intersect(templatePermissionCodes)
                .ToList();

            foreach (var code in commonPermissions)
            {
                comparisons.Add(new PermissionComparison
                {
                    PermissionCode = code,
                    Status = ComparisonStatus.Match,
                    InTemplate = true,
                    InRole = true
                });
            }

            return comparisons;
        }

        private async Task UpdateTemplateUsageAsync(Guid templateId)
        {
            if (!templateId.ToString().StartsWith("00000000-0000-0000-0000"))
            {
                var template = await _context.RoleTemplates.FindAsync(templateId);
                if (template != null)
                {
                    template.UsageCount++;
                    template.LastUsed = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<RoleTemplateDto> UpdateTemplateAsync(Guid templateId, UpdateTemplateRequest request)
        {
            // Validação básica
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Nome do template é obrigatório");

            // Em produção, buscar do banco de dados e atualizar
            // var template = await _context.RoleTemplates.FindAsync(templateId);

            return new RoleTemplateDto
            {
                Id = templateId,
                Name = request.Name,
                DisplayName = request.DisplayName,
                Description = request.Description,
                IsSystemTemplate = false,
                PermissionCodes = request.PermissionCodes ?? new List<string>(),
                Category = request.Category,
                Tags = request.Tags ?? new List<string>(),
                UsageCount = 0,
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<bool> DeleteTemplateAsync(Guid templateId)
        {
            // Em produção, buscar e marcar como inativo ou deletar
            // var template = await _context.RoleTemplates.FindAsync(templateId);
            // if (template != null) 
            // {
            //     template.IsActive = false;
            //     await _context.SaveChangesAsync();
            // }

            return await Task.FromResult(true);
        }

        private RoleTemplateDto MapToDto(RoleTemplate template)
        {
            return new RoleTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                DisplayName = template.DisplayName,
                Description = template.Description,
                IsSystemTemplate = false,
                PermissionCodes = template.PermissionCodes,
                Category = template.Category,
                Tags = template.Tags,
                UsageCount = template.UsageCount,
                LastUsed = template.LastUsed,
                CreatedAt = template.CreatedAt
            };
        }
    }
}
