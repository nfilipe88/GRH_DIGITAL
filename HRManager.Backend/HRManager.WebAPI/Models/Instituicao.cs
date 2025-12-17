using HRManager.WebAPI.Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.Models
{
    public class Instituicao : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;
        [MaxLength(15)]
        public string NIF { get; set; } = string.Empty;
        [Required]
        [MaxLength(50)]
        // Identificador Único (Slug) para URLs ou subdomínios (RN-01.1)
        public string IdentificadorUnico { get; set; }=string.Empty;
        public string Endereco { get; set; }=string.Empty;
        [DataType(DataType.PhoneNumber)]
        public int? Telemovel { get; set; }
        public string EmailContato { get; set; }=string.Empty;
        public DateTime DataCriacao { get; set; }
        public bool IsAtiva { get; set; } = true; // (FA-1)
        // Relações (para EF Core)
        public ICollection<User> Users { get; set; }=new List<User>();
        public ICollection<Colaborador> Colaboradores { get; set; } = new List<Colaborador>();
    }
}
