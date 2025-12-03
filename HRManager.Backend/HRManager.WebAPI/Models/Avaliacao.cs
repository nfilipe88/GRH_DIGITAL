using HRManager.WebAPI.Domain.enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HRManager.WebAPI.Models
{
    public class Avaliacao
    {
        [Key]
        public int Id { get; set; }

        // --- Relações ---
        public int CicloId { get; set; }
        public virtual CicloAvaliacao Ciclo { get; set; }

        public int ColaboradorId { get; set; }
        public virtual Colaborador Colaborador { get; set; }

        public int GestorId { get; set; } // Quem vai avaliar (geralmente o utilizador GestorRH)
        // Nota: Como o Gestor é um User e não um Colaborador no nosso sistema atual, 
        // guardamos o ID, mas a navegação pode ser complexa se não mapearmos User.

        // --- Estado ---
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EstadoAvaliacao Estado { get; set; } = EstadoAvaliacao.NaoIniciada;

        // --- Resultados Finais ---
        public decimal? MediaFinal { get; set; } // Calculada no fim
        public string? ComentarioFinalGestor { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataConclusao { get; set; }

        // --- Itens ---
        public virtual ICollection<AvaliacaoItem> Itens { get; set; }
    }
}
