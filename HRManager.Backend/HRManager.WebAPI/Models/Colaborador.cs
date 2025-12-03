using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManager.WebAPI.Models
{
    public class Colaborador
    {
        [Key] // Define a chave primária
        public int Id { get; set; }

        // --- Secção 1: Dados Pessoais ---
        [Required]
        [MaxLength(200)]
        public string NomeCompleto { get; set; }

        [Required]
        [MaxLength(20)]
        public string NIF { get; set; } // Número de Identificação Fiscal 

        public int? NumeroAgente { get; set; } // Opcional por agora 

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string EmailPessoal { get; set; }
        public DateTime? DataNascimento { get; set; } // 

        // --- Secção 2: Dados Contratuais ---
        [Required]
        public DateTime DataAdmissao { get; set; }

        [MaxLength(100)]
        public string Cargo { get; set; }

        [MaxLength(100)]
        public string TipoContrato { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalarioBase { get; set; }

        // --- Secção 3: Organização ---
        [MaxLength(100)]
        public string Departamento { get; set; }

        [MaxLength(100)]
        public string Localizacao { get; set; }
        // *** ADICIONE ESTAS LINHAS ***
        [MaxLength(200)]
        public string? Morada { get; set; }
        public int? Telemovel { get; set; }

        [MaxLength(34)] // Tamanho padrão do IBAN
        public string? IBAN { get; set; }
        // ******************************
        public int SaldoFerias { get; set; } = 22; // Padrão de 22 dias úteis
        // Define se o colaborador está ativo ou inativo (Soft Delete)
        public bool IsAtivo { get; set; } = true;
        // (Vamos omitir o Gestor Hierárquico no MVP para simplificar)

        //[cite_start]// Chave Estrangeira para a Instituição [cite: 80]
        public Guid InstituicaoId { get; set; }

        [ForeignKey("InstituicaoId")]
        public virtual Instituicao Instituicao { get; set; }
    }
}
