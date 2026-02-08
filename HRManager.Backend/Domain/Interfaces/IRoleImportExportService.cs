using HRManager.WebAPI.Domain.enums;
using HRManager.WebAPI.DTOs;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IRoleImportExportService
    {
        Task<byte[]> ExportRolesAsync(List<Guid> roleIds, bool includeSystemRoles = false);
        Task<byte[]> ExportAllRolesAsync();
        Task<RoleImportResult> ImportRolesAsync(RoleImportRequest request, Guid? instituicaoId = null);
        Task<RoleImportResult> ImportRolesFromJsonAsync(string jsonContent, ImportStrategy strategy = ImportStrategy.Merge);
        Task<RoleTemplateDto> ExportRoleAsTemplateAsync(Guid roleId, string templateName);
    }
}
