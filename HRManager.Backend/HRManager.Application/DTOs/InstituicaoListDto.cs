using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class InstituicaoListDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string NIF { get; set; } = string.Empty;
        // Identificador Único (Slug) para URLs ou subdomínios (RN-01.1)
        public string IdentificadorUnico { get; set; } = string.Empty;
        public string EmailContato { get; set; } = string.Empty;
        public bool IsAtiva { get; set; } = true; // (FA-1)
        public string Endereco { get; internal set; } = string.Empty;
        public int? Telemovel { get; internal set; }
    }
}
