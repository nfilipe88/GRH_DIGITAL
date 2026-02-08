namespace HRManager.WebAPI.DTOs
{
    public class UpdateRoleRequest : CreateRoleRequest
    {
        public bool IsActive { get; set; }
    }
}
