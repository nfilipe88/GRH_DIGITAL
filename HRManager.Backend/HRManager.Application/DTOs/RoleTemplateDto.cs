namespace HRManager.WebAPI.DTOs
{
    public class RoleTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSystemTemplate { get; set; }
        public List<string> PermissionCodes { get; set; } = new();
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public int UsageCount { get; set; }
        public DateTime? LastUsed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
