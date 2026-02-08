namespace HRManager.WebAPI.Helpers
{
    public class Recommendation
    {
        public string Type { get; set; } = string.Empty; // ADD, REMOVE, REPLACE
        public string PermissionCode { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Severity { get; set; } = "INFO";
    }
}
