namespace HRManager.WebAPI.DTOs
{
    public class ValidateRoleRequest
    {
        public Guid RoleId { get; set; }
        public List<Guid> PermissionIds { get; set; } = new();
    }
}
