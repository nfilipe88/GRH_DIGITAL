using HRManager.WebAPI.Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.Models
{
    public class User: TenantEntity
    {
        [Required]
        public string NomeCompleto { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsAtivo { get; set; } = true;
        // Propriedade de navegação para a Instituição
        public Instituicao? Instituicao { get; set; }
        // Relacionamento Muitos-para-Muitos com Roles
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}