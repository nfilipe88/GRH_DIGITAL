using HRManager.WebAPI.Domain.enums;
using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.Models
{
    public class Competencia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } // Ex: "Proatividade"

        public string? Descricao { get; set; } // Ex: "Capacidade de antecipar problemas..."

        public TipoCompetencia Tipo { get; set; }
        public bool IsAtiva { get; set; }

        // Multi-tenant
        public Guid InstituicaoId { get; set; }
    }
}
