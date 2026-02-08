using HRManager.WebAPI.Domain.enums;

namespace HRManager.WebAPI.DTOs
{
    public class RoleImportRequest
    {
        public List<RoleExportDto> Roles { get; set; } = new();
        public bool OverwriteExisting { get; set; } = false;
        public bool SkipSystemRoles { get; set; } = true;
        public ImportStrategy Strategy { get; set; } = ImportStrategy.Merge;
        public Dictionary<string, object>? ImportMetadata { get; set; }
    }
}
