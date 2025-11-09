using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class CriarInstituicaoRequest
    {
        // Usamos Data Annotations para validação automática
        [Required(ErrorMessage = "O nome é obrigatório")]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O IdentificadorUnico é obrigatório")]
        [MaxLength(50)]
        [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "O IdentificadorUnico deve conter apenas letras minúsculas, números e hífenes")]
        public string IdentificadorUnico { get; set; }
    }
}
