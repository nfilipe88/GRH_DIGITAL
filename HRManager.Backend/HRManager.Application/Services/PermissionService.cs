using FluentValidation;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly HRManagerDbContext _context;

        public PermissionService(HRManagerDbContext context)
        {
            _context = context;
        }

        public async Task<List<PermissionDto>> GetAllPermissionsAsync()
        {
            return await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Category)
                .ThenBy(p => p.Name)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<List<PermissionDto>> GetPermissionsByModuleAsync(string module)
        {
            return await _context.Permissions
                .Where(p => p.Module == module && p.IsActive)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task<PermissionDto> GetPermissionAsync(Guid id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) throw new KeyNotFoundException("Permissão não encontrada");
            return MapToDto(permission);
        }

        public async Task<PermissionDto> CreatePermissionAsync(CreatePermissionRequest request)
        {
            var existing = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Code == request.Code);

            if (existing != null)
                throw new ValidationException($"Já existe uma permissão com o código '{request.Code}'");

            var permission = new Permission
            {
                Id = Guid.NewGuid(),
                Code = request.Code.ToUpper(),
                Name = request.Name,
                Module = request.Module,
                Category = request.Category,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
            //_cacheService.RemoveAsync($"permissions_{roleId}")
            return MapToDto(permission);
        }

        public async Task<PermissionDto> UpdatePermissionAsync(Guid id, UpdatePermissionRequest request)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
                throw new KeyNotFoundException("Permissão não encontrada");

            permission.Name = request.Name;
            permission.Module = request.Module;
            permission.Category = request.Category;
            permission.Description = request.Description;
            permission.IsActive = request.IsActive;

            await _context.SaveChangesAsync();
            return MapToDto(permission);
        }

        public async Task<bool> TogglePermissionStatusAsync(Guid id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
                throw new KeyNotFoundException("Permissão não encontrada");

            permission.IsActive = !permission.IsActive;
            await _context.SaveChangesAsync();
            return permission.IsActive;
        }

        private PermissionDto MapToDto(Permission permission)
        {
            return new PermissionDto
            {
                Id = permission.Id,
                Code = permission.Code,
                Name = permission.Name,
                Module = permission.Module,
                Category = permission.Category,
                Description = permission.Description,
                IsActive = permission.IsActive
            };
        }
    }
}
