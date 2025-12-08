using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class AtualizarInstituicaoRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [MaxLength(100)]
        public string Nome { get; set; }

        public string Endereco { get; set; }
        [DataType(DataType.PhoneNumber)]
        public int? Telemovel { get; set; }
        [DataType(DataType.EmailAddress)]
        public string EmailContato { get; set; }
        [Required(ErrorMessage = "O identificador é obrigatório")]
        [MaxLength(50)]
        [RegularExpression("^[A-Z0-9-]+$", ErrorMessage = "O identificador deve conter apenas letras maiúsculas, números e hífenes")]
        public string IdentificadorUnico { get; set; }
        public string NIF { get; internal set; }
    }
}
