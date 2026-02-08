using FluentValidation;
using HRManager.WebAPI.Domain.enums;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Infrastructure.Caching;
using HRManager.WebAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Services
{
    public class RoleService : IRoleService
    {
        private readonly HRManagerDbContext _context;
        private readonly RoleManager<Role> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditService _auditService;
        private readonly IPermissionCacheService _cacheService;

        public RoleService(
            HRManagerDbContext context,
            RoleManager<Role> roleManager,
            IHttpContextAccessor httpContextAccessor,
            IAuditService auditService,
        IPermissionCacheService cacheService)
        {
            _context = context;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _auditService = auditService;
            _cacheService = cacheService;
        }

        public async Task<RoleDetailDto> CreateRoleAsync(CreateRoleRequest request)
        {
            // Verificar se role já existe
            var existing = await _roleManager.FindByNameAsync(request.Name);
            if (existing != null)
                throw new ValidationException($"Já existe uma role com o nome '{request.Name}'");

            var user = _httpContextAccessor.HttpContext?.User;
            var userInstituicaoId = user?.FindFirst("tenantId")?.Value;
            Guid? instituicaoId = null;

            if (Guid.TryParse(userInstituicaoId, out var parsedId))
            {
                instituicaoId = parsedId;
            }

            // Criar role
            var role = new Role
            {
                Name = request.Name,
                Description = request.Description,
                NormalizedName = request.Name.ToUpper(),
                IsSystemRole = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                InstituicaoId = instituicaoId // Roles podem ser específicas da instituição
            };

            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
                throw new ValidationException($"Erro ao criar role: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            // Atribuir permissões se fornecidas
            if (request.PermissionIds?.Any() == true)
            {
                await AssignPermissionsToRoleAsync(role.Id, request.PermissionIds);
            }

            // Registrar log
            await _auditService.LogRoleChangeAsync(role.Id, AuditAction.Created, null, new
            {
                role.Name,
                role.Description,
                role.IsSystemRole,
                PermissionIds = request.PermissionIds
            }, $"Role '{role.Name}' criada");

            // Invalidar cache
            await _cacheService.RemoveRolePermissionsCacheAsync(role.Id);

            return await GetRoleAsync(role.Id);
        }

        public async Task<bool> AssignPermissionsToRoleAsync(Guid roleId, List<Guid> permissionIds)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
                throw new KeyNotFoundException("Role não encontrada");

            // Verificar se é role do sistema (não pode modificar)
            if (role.IsSystemRole)
                throw new UnauthorizedAccessException("Não é possível modificar permissões de roles do sistema");

            // Obter permissões existentes
            var existingPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList();

            // Adicionar novas permissões
            var newPermissionIds = permissionIds.Except(existingPermissionIds).ToList();

            foreach (var permissionId in newPermissionIds)
            {
                await _auditService.LogRolePermissionChangeAsync(
                    roleId,
                    permissionId,
                    AuditAction.PermissionAssigned,
                    $"Permissão {permissionId} atribuída à role {roleId}"
                );
            }

            // Invalidar caches
            await _cacheService.RemoveRolePermissionsCacheAsync(roleId);
            await _cacheService.RemoveAllPermissionsCacheAsync();

            await _context.SaveChangesAsync();
            return true; // Ou return await Task.FromResult(true);
        }

        public async Task<RoleDetailDto> GetRoleAsync(Guid id)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .Include(r => r.Instituicao)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                throw new KeyNotFoundException("Role não encontrada");

            var dto = new RoleDetailDto
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                IsActive = role.IsActive,
                InstituicaoNome = role.Instituicao?.Nome ?? string.Empty,
                Permissions = role.RolePermissions
                    .Select(rp => new PermissionDto
                    {
                        Id = rp.Permission.Id,
                        Code = rp.Permission.Code,
                        Name = rp.Permission.Name,
                        Module = rp.Permission.Module,
                        Category = rp.Permission.Category
                    })
                    .ToList()
            };

            return dto;
        }

        public Task<PagedResult<RoleDto>> GetRolesAsync(int page = 1, int pageSize = 20)
        {
            throw new NotImplementedException();
        }

        public Task<RoleDetailDto> GetRoleByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<RoleDetailDto> UpdateRoleAsync(Guid id, UpdateRoleRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteRoleAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemovePermissionsFromRoleAsync(Guid roleId, List<Guid> permissionIds)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
                throw new KeyNotFoundException("Role não encontrada");

            if (role.IsSystemRole)
                throw new UnauthorizedAccessException("Não é possível modificar permissões de roles do sistema");

            // Obter as permissões que serão removidas
            var permissionsToRemove = role.RolePermissions
                .Where(rp => permissionIds.Contains(rp.PermissionId))
                .ToList();

            // Registrar log para cada permissão removida
            foreach (var rolePermission in permissionsToRemove)
            {
                await _auditService.LogRolePermissionChangeAsync(
                    roleId,
                    rolePermission.PermissionId,
                    AuditAction.PermissionRemoved,
                    $"Permissão {rolePermission.PermissionId} removida da role {roleId}"
                );
            }

            // Remover as permissões
            foreach (var rolePermission in permissionsToRemove)
            {
                _context.RolePermissions.Remove(rolePermission);
            }

            await _context.SaveChangesAsync();

            // Invalidar caches
            await _cacheService.RemoveRolePermissionsCacheAsync(roleId);
            await _cacheService.RemoveAllPermissionsCacheAsync();

            return true; // Ou return await Task.FromResult(true);
        }

        public Task<List<PermissionDto>> GetRolePermissionsAsync(Guid roleId)
        {
            throw new NotImplementedException();
        }
    }
}
