using HRManager.WebAPI.Domain.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HRManager.WebAPI.Models
{
    public enum GrauAcademico
    {
        EnsinoSecundario,
        Bacharelato,
        Licenciatura,
        Mestrado,
        Doutoramento,
        PosGraduacao,
        Outro
    }

    public class HabilitacaoLiteraria : TenantEntity
    {
        // --- Relação com Colaborador ---
        [Required]
        public Guid ColaboradorId { get; set; }

        [ForeignKey("ColaboradorId")]
        [JsonIgnore] // Evita ciclos ao serializar
        public virtual Colaborador Colaborador { get; set; }

        // --- Dados da Habilitação ---
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GrauAcademico Grau { get; set; }

        [Required]
        [MaxLength(200)]
        public string Curso { get; set; } = string.Empty; // Ex: Engenharia Informática

        [Required]
        [MaxLength(200)]
        public string InstituicaoEnsino { get; set; }=string.Empty; // Ex: Universidade de Luanda

        [Required]
        public DateTime DataConclusao { get; set; }

        // --- Documento Comprovativo ---
        [MaxLength(500)]
        public string? CaminhoDocumento { get; set; } // Ex: "uploads/diploma_123.pdf"
    }
}
