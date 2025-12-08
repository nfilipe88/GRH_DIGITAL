using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManager.WebAPI.DTOs
{
    public class UserDetailsDto
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; }
        public string NIF { get; set; }
        public int? NumeroAgente { get; internal set; }
        public string Email { get; set; }
        public int? Telemovel { get; set; }
        public string Cargo { get; set; }
        public DateTime? DataNascimento { get; set; } 
        public DateTime DataAdmissao { get; set; }
        public string TipoContrato { get; set; }
        public string Morada { get; set; }
        public decimal? SalarioBase { get; set; }
        public string IBAN { get; set; }
        public string Departamento { get; set; }
        public string Localizacao { get; set; }
        public int SaldoFerias { get; set; }
        public bool IsAtivo { get; set; }
        public Guid InstituicaoId { get; set; }
        public string? InstituicaoNome { get; set; } // Útil para mostrar no menu "Logado como..."}
    }
}
