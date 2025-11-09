using HRManager.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.Models
{
    public class Instituicao
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Chave principal (TenantId)

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Required]
        [MaxLength(50)]
        // Identificador Único (Slug) para URLs ou subdomínios (RN-01.1)
        public string IdentificadorUnico { get; set; }

        public bool IsAtiva { get; set; } = true; // (FA-1)

        // Relações (para EF Core)
        public ICollection<Colaborador> Colaboradores { get; set; }
    }
}
