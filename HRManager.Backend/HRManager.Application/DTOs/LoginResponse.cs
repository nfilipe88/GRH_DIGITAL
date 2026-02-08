namespace HRManager.WebAPI.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public bool MustChangePassword { get; set; }
        public string NomeUser { get; set; } = string.Empty; // Útil para mostrar "Olá, João"
        public string Email { get; set; } = string.Empty;
    }
}