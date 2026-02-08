using HRManager.WebAPI.Domain.enums;

namespace HRManager.WebAPI.DTOs
{
    public class RoleImportDetail
    {
        public string RoleName { get; set; } = string.Empty;
        public ImportStatus Status { get; set; }
        public string? Message { get; set; }
        public List<string>? Warnings { get; set; }
        public List<string>? Errors { get; set; }
    }
}
