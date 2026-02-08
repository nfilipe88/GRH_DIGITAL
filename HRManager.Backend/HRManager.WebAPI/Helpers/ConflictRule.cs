namespace HRManager.WebAPI.Helpers
{
    public class ConflictRule
    {
        public string PermissionA { get; set; } = string.Empty;
        public string PermissionB { get; set; } = string.Empty;
        public string ConflictType { get; set; } = string.Empty;
        public string Severity { get; set; } = "ERROR";
        public string Description { get; set; } = string.Empty;
    }
}
