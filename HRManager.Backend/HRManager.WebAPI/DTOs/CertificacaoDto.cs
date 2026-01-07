namespace HRManager.WebAPI.DTOs
{
    public class CertificacaoDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Entidade { get; set; } = string.Empty;
        public DateTime DataEmissao { get; set; }
        public DateTime? DataValidade { get; set; }
        public string? CaminhoDocumento { get; set; }
    }
}
