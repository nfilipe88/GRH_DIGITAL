namespace HRManager.WebAPI.DTOs
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSystemRole { get; set; }
        public bool IsActive { get; set; }
        public string InstituicaoNome { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public int PermissionCount { get; set; }
    }
}
