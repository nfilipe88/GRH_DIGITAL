using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.Models
{
    // User precisa de InstituicaoId, mas não de herdar BaseEntity 
    // se quisermos um modelo de utilizador mais isolado ou se o Gestor Master 
    // não pertencer a uma única instituição. Para simplificar, vamos associá-lo.
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        [EmailAddress]
        public string Email { get; set; } // O Email será o nosso Username

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } // Ex: "GestorMaster", "GestorRH"

        // Relação (Opcional, mas útil): A que instituição este utilizador pertence?
        // Pode ser nulo se for um Gestor Master
        public Guid? InstituicaoId { get; set; }
    }
}