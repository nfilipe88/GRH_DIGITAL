namespace HRManager.WebAPI.DTOs
{
    public class HabilitacaoDto
    {
        public Guid Id { get; set; }
        public string Grau { get; set; } = string.Empty;
        public string Curso { get; set; } = string.Empty;
        public string Instituicao { get; set; } = string.Empty;
        public DateTime DataConclusao { get; set; }
        public string? CaminhoDocumento { get; set; }
    }
}
