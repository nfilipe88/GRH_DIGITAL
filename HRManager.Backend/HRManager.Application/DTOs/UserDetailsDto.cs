using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManager.WebAPI.DTOs
{
    public class UserDetailsDto
    {
        public Guid Id { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string NIF { get; set; } = string.Empty;
        public int? NumeroAgente { get; internal set; }
        public string Email { get; set; } = string.Empty;
        public int? Telemovel { get; set; }
        public string Cargo { get; set; } = String.Empty;
        public DateTime? DataNascimento { get; set; } 
        public DateTime DataAdmissao { get; set; }
        public string TipoContrato { get; set; }= String.Empty;
        public string Morada { get; set; }= String.Empty;
        public decimal? SalarioBase { get; set; }
        public string IBAN { get; set; }= String.Empty;
        public string Departamento { get; set; }= String.Empty;
        public string Localizacao { get; set; }= String.Empty;
        public int SaldoFerias { get; set; }
        public bool IsAtivo { get; set; }
        public Guid InstituicaoId { get; set; }
        public string? InstituicaoNome { get; set; } // Útil para mostrar no menu "Logado como..."}
    }
}
