namespace HRManager.WebAPI.DTOs
{
    public class CertificacaoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Entidade { get; set; }
        public DateTime DataEmissao { get; set; }
        public DateTime? DataValidade { get; set; }
        public string? CaminhoDocumento { get; set; }
    }
}
