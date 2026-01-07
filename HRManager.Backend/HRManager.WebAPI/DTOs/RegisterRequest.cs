using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class RegisterRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }=String.Empty;
        public string Nome { get; set; }= String.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = String.Empty;

        [Required]
        public string Role { get; set; } = String.Empty; // "GestorMaster" ou "GestorRH"

        public Guid? InstituicaoId { get; set; } // Opcional
    }
}
