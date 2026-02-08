namespace HRManager.WebAPI.DTOs
{
    public class CreateTemplateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> PermissionCodes { get; set; } = new();
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public Guid CreatedBy { get; set; }
    }
}
