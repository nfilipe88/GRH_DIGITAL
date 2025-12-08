using HRManager.WebAPI.Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.Models
{
    // User precisa de InstituicaoId, mas não de herdar BaseEntity 
    // se quisermos um modelo de utilizador mais isolado ou se o Gestor Master 
    // não pertencer a uma única instituição. Para simplificar, vamos associá-lo.
    public class User: BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsAtivo { get; set; } = true;

        // Assumindo que o User tem ligação à Instituição (Multi-tenant)
        public Guid? InstituicaoId { get; set; }
        public Instituicao? Instituicao { get; set; }
    }
}