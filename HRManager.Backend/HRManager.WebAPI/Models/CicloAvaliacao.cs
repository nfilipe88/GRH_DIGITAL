using HRManager.WebAPI.Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.Models
{
    public class CicloAvaliacao : TenantEntity
    {
        [Required]
        public string Nome { get; set; } = String.Empty; // Ex: "Avaliação 2025"
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public bool IsAtivo { get; set; } = true;
    }
}
