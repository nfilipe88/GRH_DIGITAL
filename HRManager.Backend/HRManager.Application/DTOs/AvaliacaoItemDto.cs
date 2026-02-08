namespace HRManager.WebAPI.DTOs
{
    public class AvaliacaoItemDto
    {
        public Guid Id { get; set; }
        public Guid CompetenciaId { get; set; }
        public string NomeCompetencia { get; set; } = string.Empty; // ex: "Trabalho em Equipa"
        public string DescricaoCompetencia { get; set; } = string.Empty;
        public int? NotaColaborador { get; set; }
        public int? NotaGestor { get; set; }
        public string? Comentario { get; set; }
        public int? NotaAutoAvaliacao { get; internal set; }
        public string? JustificativaColaborador { get; internal set; }
    }
}
