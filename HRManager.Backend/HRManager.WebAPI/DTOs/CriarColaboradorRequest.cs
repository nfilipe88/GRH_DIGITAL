using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class CriarColaboradorRequest
    {
        // --- Dados Pessoais ---
        [Required]
        public string NomeCompleto { get; set; } = string.Empty;
        [Required]
        public string NIF { get; set; } = string.Empty;
        public int? NumeroAgente { get; set; }
        [Required]
        [EmailAddress]
        public string EmailPessoal { get; set; } = string.Empty;
        public string Morada { get; set; } = string.Empty;
        public int? Telemovel { get; set; }
        public DateTime? DataNascimento { get; set; }

        // --- Dados Contratuais ---
        [Required]
        public DateTime DataAdmissao { get; set; }

        // Alterado de string Cargo para Guid CargoId
        [Required]
        public Guid CargoId { get; set; }

        public string TipoContrato { get; set; } = "Sem Termo";
        public decimal SalarioBase { get; set; }
        public string IBAN { get; set; } = string.Empty;

        // --- Organização ---
        public string Departamento { get; set; } = string.Empty;
        public string Localizacao { get; set; } = "Sede";

        public Guid? InstituicaoId { get; set; }
    }
}