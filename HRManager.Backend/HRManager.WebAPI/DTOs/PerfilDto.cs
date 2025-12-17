namespace HRManager.WebAPI.DTOs
{
    public class PerfilDto
    {
        // Dados Básicos
        public Guid ColaboradorId { get; set; }
        public string NomeCompleto { get; set; } = String.Empty;
        public string Cargo { get; set; }=String.Empty;
        public string Email { get; set; }=String.Empty;
        // *** ADICIONE ESTAS LINHAS ***
        public string? Morada { get; set; }
        public string? IBAN { get; set; }
        // ******************************

        // Listas Curriculares
        public List<HabilitacaoDto> Habilitacoes { get; set; }
        public List<CertificacaoDto> Certificacoes { get; set; }
    }
}
