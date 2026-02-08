using HRManager.WebAPI.DTOs;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IRoleTemplateService
    {
        Task<List<RoleTemplateDto>> GetSystemTemplatesAsync();
        Task<List<RoleTemplateDto>> GetUserTemplatesAsync(Guid userId);
        Task<RoleTemplateDto> CreateTemplateAsync(CreateTemplateRequest request);
        Task<RoleTemplateDto> UpdateTemplateAsync(Guid templateId, UpdateTemplateRequest request);
        Task<bool> DeleteTemplateAsync(Guid templateId);
        Task<RoleDetailDto> ApplyTemplateToRoleAsync(Guid roleId, Guid templateId);
        Task<RoleDetailDto> CreateRoleFromTemplateAsync(Guid templateId, string roleName, string? description = null);
        Task<List<PermissionComparison>> CompareRoleWithTemplateAsync(Guid roleId, Guid templateId);
    }
}
