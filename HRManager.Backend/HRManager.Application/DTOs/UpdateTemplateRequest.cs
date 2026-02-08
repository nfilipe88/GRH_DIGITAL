namespace HRManager.WebAPI.DTOs
{
    public class UpdateTemplateRequest : CreateTemplateRequest
    {
        public bool IsActive { get; set; }
    }
}
