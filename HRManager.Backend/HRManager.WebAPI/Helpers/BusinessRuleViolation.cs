namespace HRManager.WebAPI.Helpers
{
    public class BusinessRuleViolation
    {
        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = "ERROR";
        public List<string> AffectedPermissions { get; set; } = new();
    }
}
