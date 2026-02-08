namespace HRManager.WebAPI.DTOs
{
    public class UpdatePermissionRequest : CreatePermissionRequest
    {
        public bool IsActive { get; set; }
    }
}
