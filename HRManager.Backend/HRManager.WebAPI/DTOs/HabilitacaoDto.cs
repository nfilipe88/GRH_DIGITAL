namespace HRManager.WebAPI.DTOs
{
    public class HabilitacaoDto
    {
        public int Id { get; set; }
        public string Grau { get; set; }
        public string Curso { get; set; }
        public string Instituicao { get; set; }
        public DateTime DataConclusao { get; set; }
        public string? CaminhoDocumento { get; set; }
    }
}
