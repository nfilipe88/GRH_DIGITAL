namespace HRManager.WebAPI.DTOs
{
    public class ExportRolesRequest
    {
        public List<Guid> RoleIds { get; set; } = new();
        public bool IncludeSystemRoles { get; set; } = false;
    }
}
