using HRManager.WebAPI.Domain.Base;
using HRManager.WebAPI.Domain.enums;
using System.Text.Json.Serialization;

namespace HRManager.WebAPI.Models
{
    public class Avaliacao : TenantEntity
    {
        public Guid CicloId { get; set; }
        public virtual CicloAvaliacao Ciclo { get; set; } = null!;
        public Guid ColaboradorId { get; set; }
        public virtual Colaborador Colaborador { get; set; } = null!;
        public Guid GestorId { get; set; }
        public virtual User? Gestor { get; set; }
        public virtual Instituicao? Instituicao { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EstadoAvaliacao Estado { get; set; } = EstadoAvaliacao.NaoIniciada;
        public decimal? MediaFinal { get; set; }
        public string? ComentarioFinalGestor { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataConclusao { get; set; }
        public virtual ICollection<AvaliacaoItem> Itens { get; set; } = new List<AvaliacaoItem>();
    }
}