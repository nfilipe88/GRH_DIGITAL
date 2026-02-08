namespace HRManager.WebAPI.DTOs
{
    public class UserSimpleDto
    {
        public Guid Id { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
