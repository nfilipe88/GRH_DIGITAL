namespace HRManager.WebAPI.DTOs
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Changes { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, object>? OldValues { get; set; }
        public Dictionary<string, object>? NewValues { get; set; }
    }
}
