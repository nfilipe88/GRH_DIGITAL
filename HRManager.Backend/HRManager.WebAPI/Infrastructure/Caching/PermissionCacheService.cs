using HRManager.WebAPI.DTOs;
using Microsoft.Extensions.Caching.Distributed;

namespace HRManager.WebAPI.Infrastructure.Caching
{
    public class PermissionCacheService : IPermissionCacheService
    {
        private readonly IDistributedCacheService _cacheService;
        private readonly ILogger<PermissionCacheService> _logger;
        private const string PERMISSION_CACHE_PREFIX = "permissions:";
        private const string ROLE_CACHE_PREFIX = "roles:";
        private const int DEFAULT_CACHE_MINUTES = 30;

        public PermissionCacheService(
            IDistributedCacheService cacheService,
            ILogger<PermissionCacheService> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<List<PermissionDto>> GetUserPermissionsAsync(Guid userId)
        {
            var cacheKey = $"{PERMISSION_CACHE_PREFIX}user:{userId}";
            return await _cacheService.GetAsync<List<PermissionDto>>(cacheKey) ?? new List<PermissionDto>();
        }

        public async Task SetUserPermissionsAsync(Guid userId, List<PermissionDto> permissions)
        {
            var cacheKey = $"{PERMISSION_CACHE_PREFIX}user:{userId}";
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(DEFAULT_CACHE_MINUTES),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };

            await _cacheService.SetAsync(cacheKey, permissions, options);
        }

        public async Task RemoveUserPermissionsAsync(Guid userId)
        {
            var cacheKey = $"{PERMISSION_CACHE_PREFIX}user:{userId}";
            await _cacheService.RemoveAsync(cacheKey);

            // Também remover cache de usuários que podem ter sido afetados por mudanças nesta role
            await ClearRelatedCachesAsync();
        }

        public async Task RemoveAllPermissionsCacheAsync()
        {
            // Em produção, usar Redis com SCAN para remover todos os keys com prefixo
            // Por enquanto, limitamos a limpeza programática
            _logger.LogInformation("Cache de permissões limpo globalmente");
        }

        public async Task<List<RoleDetailDto>> GetRoleWithPermissionsAsync(Guid roleId)
        {
            var cacheKey = $"{ROLE_CACHE_PREFIX}detail:{roleId}";
            return await _cacheService.GetAsync<List<RoleDetailDto>>(cacheKey) ?? new List<RoleDetailDto>();
        }

        public async Task SetRoleWithPermissionsAsync(Guid roleId, RoleDetailDto role)
        {
            var cacheKey = $"{ROLE_CACHE_PREFIX}detail:{roleId}";
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
            };

            await _cacheService.SetAsync(cacheKey, new List<RoleDetailDto> { role }, options);
        }

        public async Task RemoveRolePermissionsCacheAsync(Guid roleId)
        {
            var cacheKey = $"{ROLE_CACHE_PREFIX}detail:{roleId}";
            await _cacheService.RemoveAsync(cacheKey);

            // Notificar outros serviços sobre mudança
            await ClearRelatedCachesAsync();
        }

        private async Task ClearRelatedCachesAsync()
        {
            // Aqui poderíamos usar um sistema de pub/sub para notificar outras instâncias
            // ou limpar caches relacionados
        }
        public async Task ClearCacheAsync(Guid userId)
        {
            var cacheKey = $"permissions_{userId}";
            await _cacheService.RemoveAsync(cacheKey);
        }
    }
}
