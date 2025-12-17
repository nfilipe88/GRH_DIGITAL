using HRManager.WebAPI.Domain.Base;
using HRManager.WebAPI.Domain.enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRManager.WebAPI.Models
{
    public class Competencia : TenantEntity
    {
        [Required]
        public string Nome { get; set; } // Ex: "Proatividade"

        public string? Descricao { get; set; } // Ex: "Capacidade de antecipar problemas..."

        public TipoCompetencia Tipo { get; set; }
        public bool IsAtiva { get; set; }

        // Multi-tenant
        [JsonIgnore]
        public virtual Instituicao? Instituicao { get; set; }
    }
}
