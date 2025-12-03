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
        public int Telefone { get; set; }
        [DataType(DataType.EmailAddress)]
        public string EmailContato { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        [Required(ErrorMessage = "O identificador é obrigatório")]
        [MaxLength(50)]
        [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "O identificador deve conter apenas letras minúsculas, números e hífenes")]
        public string IdentificadorUnico { get; set; }
    }
}
