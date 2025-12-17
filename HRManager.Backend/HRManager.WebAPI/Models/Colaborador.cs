using HRManager.WebAPI.Domain.Base;
using HRManager.WebAPI.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManager.WebAPI.Models
{
    public class Colaborador : TenantEntity
    {
        // --- Secção 1: Dados Pessoais ---
        [Required]
        [MaxLength(200)]
        public string NomeCompleto { get; set; } = string.Empty;
        [Required]
        [MaxLength(20)]
        public string NIF { get; set; } = string.Empty;
        public int? NumeroAgente { get; set; }
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string EmailPessoal { get; set; } = string.Empty;
        public DateTime? DataNascimento { get; set; }
        [Required]
        public DateTime DataAdmissao { get; set; }

        // --- RELACIONAMENTOS (Chaves Estrangeiras) ---
        
        // Cargo (Obrigatório)
        public Guid CargoId { get; set; }
        public Cargo? Cargo { get; set; }
        // User (Opcional no início, mas idealmente 1:1)
        public Guid? UserId { get; set; }
        public User? User { get; set; }
        // Gestor (Hierarquia - Opcional pois o chefe máximo não tem gestor)
        public Guid? GestorId { get; set; }
        public Colaborador? Gestor { get; set; }
        public Instituicao? Instituicao { get; set; }

        // --- Outros Dados ---
        [MaxLength(100)]
        public string TipoContrato { get; set; } = "Sem Termo";
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalarioBase { get; set; }
        [MaxLength(100)]
        public string? Departamento { get; set; } = "Geral";
        [MaxLength(100)]
        public string Localizacao { get; set; } = "Sede";
        [MaxLength(200)]
        public string? Morada { get; set; }
        public int? Telemovel { get; set; }
        [MaxLength(34)]
        public string? IBAN { get; set; }
        public int SaldoFerias { get; set; } = 22;
        public bool IsAtivo { get; set; } = true;
    }
}
