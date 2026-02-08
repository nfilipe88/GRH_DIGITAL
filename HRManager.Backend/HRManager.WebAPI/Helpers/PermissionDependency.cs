namespace HRManager.WebAPI.Helpers
{
    public class PermissionDependency
    {
        public string PermissionCode { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public string DependencyType { get; set; } = "REQUIRED"; // REQUIRED, RECOMMENDED, OPTIONAL
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = "ERROR"; // ERROR, WARNING, INFO
    }
}
