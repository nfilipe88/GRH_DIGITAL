namespace HRManager.WebAPI.DTOs
{
    public class RoleDetailDto : RoleDto
    {
        public List<PermissionDto> Permissions { get; set; } = new();
        public List<UserSimpleDto> Users { get; set; } = new();
    }
}
