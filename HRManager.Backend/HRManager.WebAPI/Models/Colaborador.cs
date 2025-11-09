using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManager.WebAPI.Models
{
    public class Colaborador
    {
        public int Id { get; set; }

        [Required]
        public string NomeCompleto { get; set; }

        [Required]
        public string NIF { get; set; } // Número de Identificação [cite: 80]
        [Required]
        public int NumeroAgente { get; set; } // Número de Identificação Único do funcionário [cite: 80]

        [Required]
        public string EmailPessoal { get; set; }

        public DateTime DataAdmissao { get; set; }

        //[cite_start]// Chave Estrangeira para a Instituição [cite: 80]
        public Guid InstituicaoId { get; set; }

        [ForeignKey("InstituicaoId")]
        public virtual Instituicao Instituicao { get; set; }
    }
}
