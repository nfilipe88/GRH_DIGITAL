namespace HRManager.WebAPI.Helpers
{
    public class PermissionCompatibilityReport
    {
        public List<string> PermissionCodes { get; set; } = new();
        public DateTime CheckedAt { get; set; }
        public bool IsCompatible { get; set; }
        public string Severity { get; set; } = "INFO";
        public List<PermissionDependency> MissingDependencies { get; set; } = new();
        public List<PermissionConflict> Conflicts { get; set; } = new();
        public CompatibilityMetrics Metrics { get; set; } = new();
        public List<Recommendation> Recommendations { get; set; } = new();
    }
}
