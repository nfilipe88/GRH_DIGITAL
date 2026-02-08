using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class CriarCicloRequest
    {
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty; //Ex: "Avaliação 2024"

        [Required]
        public DateTime DataInicio { get; set; }

        [Required]
        public DateTime DataFim { get; set; }

        // Opcional: Gestor Master pode especificar a instituição. 
        // GestorRH usa a do token.
        public Guid? InstituicaoId { get; set; }
    }
}
