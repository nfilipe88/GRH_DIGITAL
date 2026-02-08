namespace HRManager.WebAPI.Helpers
{
    public class PermissionConflict
    {
        public string PermissionCodeA { get; set; } = string.Empty;
        public string PermissionCodeB { get; set; } = string.Empty;
        public string ConflictType { get; set; } = string.Empty; // MUTUALLY_EXCLUSIVE, SEQUENCE_VIOLATION, etc.
        public string Severity { get; set; } = "ERROR";
        public string Description { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
    }
}
