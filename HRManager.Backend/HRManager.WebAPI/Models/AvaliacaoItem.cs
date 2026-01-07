using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HRManager.WebAPI.Domain.Base;

namespace HRManager.WebAPI.Models
{
    public class AvaliacaoItem : TenantEntity
    {
        public Guid AvaliacaoId { get; set; }
        [JsonIgnore]
        public virtual Avaliacao? Avaliacao { get; set; }
        public Guid CompetenciaId { get; set; }
        public virtual Competencia? Competencia { get; set; }
        // --- As Notas (1 a 5) ---
        public int? NotaAutoAvaliacao { get; set; }
        public int? NotaGestor { get; set; }
        // --- Comentários por competência ---
        public string? JustificativaColaborador { get; set; }
        public string? JustificativaGestor { get; set; }
    }
}
