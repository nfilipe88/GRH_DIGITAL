using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class CriarInstituicaoRequest
    {
        // Usamos Data Annotations para validação automática
        [Required(ErrorMessage = "O nome é obrigatório")]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;
        [Required(ErrorMessage = "O IdentificadorUnico é obrigatório")]
        [MaxLength(50)]
        [RegularExpression("^[A-Z0-9-]+$", ErrorMessage = "O IdentificadorUnico deve conter apenas letras maiúsculas, números e hífenes")]
        public string IdentificadorUnico { get; set; } = string.Empty;
        [MaxLength(15)]
        public string NIF { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public int? Telemovel { get; set; }
        public string EmailContato { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
    }
}
