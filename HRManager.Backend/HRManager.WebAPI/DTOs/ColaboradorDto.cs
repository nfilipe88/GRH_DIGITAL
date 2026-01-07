using HRManager.WebAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManager.WebAPI.DTOs
{
    public class ColaboradorDto
    {
        public Guid Id { get; set; } // Adiciona a propriedade Id para resolver CS1061
        public string? NomeCompleto { get; set; }
        public string? NIF { get; set; }
        public int? NumeroAgente { get; set; }
        public string? EmailPessoal { get; set; }
        public DateTime? DataNascimento { get; set; }
        public DateTime DataAdmissao { get; set; }
        public Guid CargoId { get; set; }
        public Cargo? Cargo { get; set; }
        public string? NomeCargo { get; set; }
        public Guid? UserId { get; set; }
        public User? User { get; set; }
        public Guid? GestorId { get; set; }
        public Colaborador? Gestor { get; set; }
        public ICollection<Colaborador>? Subordinados { get; set; }
        public Instituicao? Instituicao { get; set; }
        public string? NomeInsituicao { get; set; }
        public string? TipoContrato { get; set; }
        public decimal? SalarioBase { get; set; }
        public string? Departamento { get; set; }
        public string? Localizacao { get; set; }
        public string? Morada { get; set; }
        public int? Telemovel { get; set; }
        public string? IBAN { get; set; }
        public int SaldoFerias { get; set; }
        public bool IsAtivo { get; set; }
        public string? NomeGestor { get; internal set; }
        public Guid InstituicaoId { get; internal set; }
    }
}
