namespace HRManager.WebAPI.Helpers
{
    public class CompatibilityMetrics
    {
        public int TotalPermissions { get; set; }
        public int MissingDependenciesCount { get; set; }
        public int ConflictCount { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public double CompatibilityScore { get; set; }
    }
}
