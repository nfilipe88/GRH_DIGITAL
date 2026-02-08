namespace HRManager.WebAPI.DTOs
{
    public class RoleExportDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSystemRole { get; set; }
        public bool IsActive { get; set; }
        public List<string> PermissionCodes { get; set; } = new();
        public Dictionary<string, object>? Metadata { get; set; }
        public DateTime ExportDate { get; set; } = DateTime.UtcNow;
        public string ExportVersion { get; set; } = "1.0";
    }
}
