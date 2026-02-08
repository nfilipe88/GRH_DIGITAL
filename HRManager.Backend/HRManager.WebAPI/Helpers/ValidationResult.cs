namespace HRManager.WebAPI.Helpers
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Severity { get; set; } = "INFO"; // INFO, WARNING, ERROR
        public List<PermissionDependency> Dependencies { get; set; } = new();
        public List<PermissionConflict> Conflicts { get; set; } = new();
        public List<BusinessRuleViolation> BusinessRuleViolations { get; set; } = new();
        public bool HasErrors => Dependencies.Any(d => d.Severity == "ERROR") ||
                                Conflicts.Any(c => c.Severity == "ERROR") ||
                                BusinessRuleViolations.Any(b => b.Severity == "ERROR");
        public bool HasWarnings => Dependencies.Any(d => d.Severity == "WARNING") ||
                                  Conflicts.Any(c => c.Severity == "WARNING") ||
                                  BusinessRuleViolations.Any(b => b.Severity == "WARNING");

        public string GetOverallSeverity()
        {
            if (HasErrors) return "ERROR";
            if (HasWarnings) return "WARNING";
            return "INFO";
        }
    }
}
