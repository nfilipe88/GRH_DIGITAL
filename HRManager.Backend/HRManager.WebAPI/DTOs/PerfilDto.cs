namespace HRManager.WebAPI.DTOs
{
    public class PerfilDto
    {
        // Dados Básicos
        public int ColaboradorId { get; set; }
        public string NomeCompleto { get; set; }
        public string Cargo { get; set; }
        public string Email { get; set; }
        // *** ADICIONE ESTAS LINHAS ***
        public string? Morada { get; set; }
        public string? IBAN { get; set; }
        // ******************************

        // Listas Curriculares
        public List<HabilitacaoDto> Habilitacoes { get; set; }
        public List<CertificacaoDto> Certificacoes { get; set; }
    }
}
