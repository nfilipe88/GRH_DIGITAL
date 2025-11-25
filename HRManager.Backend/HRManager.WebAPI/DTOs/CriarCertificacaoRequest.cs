using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class CriarCertificacaoRequest
    {
        [Required]
        public string NomeCertificacao { get; set; }

        [Required]
        public string EntidadeEmissora { get; set; }

        [Required]
        public DateTime DataEmissao { get; set; }

        public DateTime? DataValidade { get; set; }

        public IFormFile? Documento { get; set; }
    }
}
