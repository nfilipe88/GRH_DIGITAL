namespace HRManager.WebAPI.Models
{
    public class RoleTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> PermissionCodes { get; set; } = new();
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public Guid CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public int UsageCount { get; set; }
        public DateTime? LastUsed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
