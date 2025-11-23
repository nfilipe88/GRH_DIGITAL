using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HRManager.WebAPI.Models
{
    // Enum para o tipo de ausência
    public enum TipoAusencia
    {
        Ferias,
        Doenca,
        Justificada,
        Outro
    }

    // Enum para o estado do pedido
    public enum EstadoAusencia
    {
        Pendente,
        Aprovada,
        Rejeitada,
        Cancelada
    }

    public class Ausencia
    {
        [Key]
        public int Id { get; set; }

        // --- Quem solicitou? ---
        [Required]
        public int ColaboradorId { get; set; }

        [ForeignKey("ColaboradorId")]
        public virtual Colaborador Colaborador { get; set; }

        // --- Detalhes do Pedido ---
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))] // Para aparecer como texto no JSON
        public TipoAusencia Tipo { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        [Required]
        public DateTime DataFim { get; set; }

        public string? Motivo { get; set; } // Opcional (ex: descrição da doença)

        // --- Controlo e Aprovação ---
        public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EstadoAusencia Estado { get; set; } = EstadoAusencia.Pendente;

        public string? ComentarioGestor { get; set; } // Ex: Motivo da rejeição

        public DateTime? DataResposta { get; set; } // Quando foi aprovado/rejeitado
    }
}
