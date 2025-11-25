using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HRManager.WebAPI.Models
{
    public enum TipoDeclaracao
    {
        FinsBancarios,      // Comprovativo de rendimentos/vínculo
        FinsEscolares,      // Estatuto trabalhador-estudante
        VistoConsular,      // Para viagens
        ComprovativoVinculo, // Genérico
        Outros
    }

    public enum EstadoPedidoDeclaracao
    {
        Pendente,
        Concluido, // O documento já foi emitido e carregado
        Rejeitado
    }

    public class PedidoDeclaracao
    {
        [Key]
        public int Id { get; set; }

        // --- Quem pediu? ---
        [Required]
        public int ColaboradorId { get; set; }

        [ForeignKey("ColaboradorId")]
        public virtual Colaborador Colaborador { get; set; }

        // --- O que pediu? ---
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TipoDeclaracao Tipo { get; set; }

        [MaxLength(500)]
        public string? Observacoes { get; set; } // Ex: "Preciso que mencione o salário anual"

        // --- Estado e Resolução ---
        public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataConclusao { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EstadoPedidoDeclaracao Estado { get; set; } = EstadoPedidoDeclaracao.Pendente;

        // --- O Documento Final ---
        [MaxLength(500)]
        public string? CaminhoFicheiro { get; set; } // O PDF final carregado pelo RH
    }
}
