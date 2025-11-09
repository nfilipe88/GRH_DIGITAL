using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.Domain.Entities
{
    // User precisa de InstituicaoId, mas não de herdar BaseEntity 
    // se quisermos um modelo de utilizador mais isolado ou se o Gestor Master 
    // não pertencer a uma única instituição. Para simplificar, vamos associá-lo.
    public class User : IHaveTenant // Implementa diretamente IHaveTenant
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Username { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; } // Segurança

        [Required]
        public byte[] PasswordSalt { get; set; } // Segurança

        public string NomeCompleto { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } // GestorMaster, TenantAdmin, Colaborador (RNF-SEC.2)

        // Relação Multi-Tenant
        public Guid InstituicaoId { get; set; } // IHaveTenant

        public Instituicao Instituicao { get; set; } = default!; // Relação 1:N
    }
}