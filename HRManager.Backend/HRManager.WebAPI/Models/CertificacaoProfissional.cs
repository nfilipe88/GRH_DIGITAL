using HRManager.WebAPI.Domain.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HRManager.WebAPI.Models
{
    public class CertificacaoProfissional : TenantEntity
    {
        // --- Relação com Colaborador ---
        [Required]
        public Guid ColaboradorId { get; set; }

        [ForeignKey("ColaboradorId")]
        [JsonIgnore]
        public virtual Colaborador? Colaborador { get; set; }

        // --- Dados da Certificação ---
        [Required]
        [MaxLength(200)]
        public string NomeCertificacao { get; set; } = string.Empty; // Ex: PMP, Cisco CCNA

        [Required]
        [MaxLength(200)]
        public string EntidadeEmissora { get; set; } = string.Empty; // Ex: PMI, Cisco

        [Required]
        public DateTime DataEmissao { get; set; }

        public DateTime? DataValidade { get; set; } // Algumas certificações expiram

        // --- Documento Comprovativo ---
        [MaxLength(500)]
        public string? CaminhoDocumento { get; set; }
    }
}
