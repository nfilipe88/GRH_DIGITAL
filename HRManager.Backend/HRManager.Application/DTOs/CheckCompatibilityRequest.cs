namespace HRManager.WebAPI.DTOs
{
    public class CheckCompatibilityRequest
    {
        public List<string> PermissionCodes { get; set; } = new();
    }
}
