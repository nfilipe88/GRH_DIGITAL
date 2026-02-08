namespace HRManager.WebAPI.DTOs
{
    public class ChangePasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordAtual { get; set; } = string.Empty; // A temporária
        public string NovaPassword { get; set; } = string.Empty;
        public string ConfirmarNovaPassword { get; set; } = string.Empty;
    }
}
