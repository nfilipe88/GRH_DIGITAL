using HRManager.WebAPI.Domain.enums;

namespace HRManager.WebAPI.DTOs
{
    public class PermissionComparison
    {
        public string PermissionCode { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public ComparisonStatus Status { get; set; }
        public bool InTemplate { get; set; }
        public bool InRole { get; set; }
    }
}
