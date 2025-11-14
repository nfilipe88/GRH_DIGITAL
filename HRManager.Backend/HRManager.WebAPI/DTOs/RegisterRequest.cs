using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class RegisterRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; } // "GestorMaster" ou "GestorRH"

        public Guid? InstituicaoId { get; set; } // Opcional
    }
}
