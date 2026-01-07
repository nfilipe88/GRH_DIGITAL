namespace HRManager.WebAPI.DTOs
{
    public class PerfilDto
    {
        // Dados Básicos
        public Guid ColaboradorId { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string Cargo { get; set; }=string.Empty;
        public string Email { get; set; }=string.Empty;
        // *** ADICIONE ESTAS LINHAS ***
        public string? Morada { get; set; }
        public string? IBAN { get; set; }
        // ******************************

        // Listas Curriculares
        public List<HabilitacaoDto>? Habilitacoes { get; set; }
        public List<CertificacaoDto>? Certificacoes { get; set; }
    }
}
