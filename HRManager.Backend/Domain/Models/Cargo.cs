using HRManager.WebAPI.Domain.Base;
using HRManager.WebAPI.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.Models
{
    public class Cargo : TenantEntity
    {
        [Required]
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public virtual Instituicao? Instituicao { get; set; }
        public bool IsAtivo { get; set; } = true;
    }
}
