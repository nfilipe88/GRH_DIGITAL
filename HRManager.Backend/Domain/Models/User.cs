using HRManager.WebAPI.Domain.Base;
using HRManager.WebAPI.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManager.WebAPI.Models
{
    public class User: IdentityUser<Guid>, IHaveTenant
    {
        [Required]
        public string NomeCompleto { get; set; } = string.Empty;
        public bool IsAtivo { get; set; } = true;
        public bool MustChangePassword { get; set; } = false;
        // Propriedade de navegação para a Instituição
        public Guid InstituicaoId { get; set; }
        [ForeignKey("InstituicaoId")]
        public Instituicao? Instituicao { get; set; }
        // Relacionamento Muitos-para-Muitos com Roles
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}