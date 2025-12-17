using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using HRManager.WebAPI.Domain.Base;

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
        Aprovado,
        Pendente,
        Concluido, // O documento já foi emitido e carregado
        Rejeitado
    }

    public class PedidoDeclaracao : BaseEntity
    {
        public Guid ColaboradorId { get; set; }
        public Colaborador Colaborador { get; set; } = null!;
        public TipoDeclaracao Tipo { get; set; }
        public string? Observacoes { get; set; }
        public EstadoPedidoDeclaracao Estado { get; set; }
        public DateTime DataSolicitacao { get; set; }
        public DateTime? DataConclusao { get; set; }
        // --- O Documento Final ---
        [MaxLength(500)]
        public string? CaminhoFicheiro { get; set; } // O PDF final carregado pelo RH
    }
}
