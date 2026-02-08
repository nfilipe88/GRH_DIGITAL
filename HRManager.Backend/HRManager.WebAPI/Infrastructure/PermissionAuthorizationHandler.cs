using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Infrastructure.Caching;
using HRManager.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRManager.WebAPI.Infrastructure
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly HRManagerDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPermissionCacheService _cacheService;
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(
            HRManagerDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IPermissionCacheService cacheService,
            ILogger<PermissionAuthorizationHandler> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _cacheService = cacheService;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
                return;

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return;

            // Verificar se é GestorMaster (acesso total)
            if (context.User.IsInRole("GestorMaster"))
            {
                context.Succeed(requirement);
                return;
            }

            // Tentar obter do cache
            var cachedPermissions = await _cacheService.GetUserPermissionsAsync(userId);

            if (!cachedPermissions.Any())
            {
                // Cache miss - buscar do banco
                var userPermissions = await GetUserPermissionsFromDbAsync(userId);

                // Armazenar no cache
                await _cacheService.SetUserPermissionsAsync(userId, userPermissions);
                cachedPermissions = userPermissions;
            }

            // Verificar se tem a permissão necessária
            if (cachedPermissions.Any(p => p.Code == requirement.PermissionCode))
            {
                context.Succeed(requirement);
            }
        }

        private async Task<List<PermissionDto>> GetUserPermissionsFromDbAsync(Guid userId)
        {
            var permissions = await _context.Users
                .Where(u => u.Id == userId)
                .SelectMany(u => u.UserRoles)
                .Select(ur => ur.Role)
                .Where(r => r.IsActive)
                .SelectMany(r => r.RolePermissions)
                .Select(rp => rp.Permission)
                .Where(p => p.IsActive)
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Module = p.Module,
                    Category = p.Category,
                    Description = p.Description
                })
                .Distinct()
                .ToListAsync();

            return permissions;
        }
    }
}
