using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class CriarColaboradorRequest
    {
        // --- Dados Pessoais ---
        [Required]
        public string NomeCompleto { get; set; }
        [Required]
        public string NIF { get; set; }
        public int? NumeroAgente { get; set; }
        [Required]
        [EmailAddress]
        public string EmailPessoal { get; set; }
        public DateTime? DataNascimento { get; set; }

        // --- Dados Contratuais ---
        [Required]
        public DateTime DataAdmissao { get; set; }
        public string Cargo { get; set; }
        public string TipoContrato { get; set; }
        public decimal SalarioBase { get; set; }

        // --- Organização ---
        public string Departamento { get; set; }
        public string Localizacao { get; set; }

        [Required] // A Instituição é obrigatória!
        public Guid InstituicaoId { get; set; }
    }
}