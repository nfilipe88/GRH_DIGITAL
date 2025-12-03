using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.Models
{
    public class CicloAvaliacao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } // Ex: "Avaliação 2025"

        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        public bool IsAtivo { get; set; } = true;

        // Multi-tenant: Ciclos pertencem a uma instituição
        public Guid InstituicaoId { get; set; }
    }
}
