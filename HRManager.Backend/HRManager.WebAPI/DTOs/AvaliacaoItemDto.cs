namespace HRManager.WebAPI.DTOs
{
    public class AvaliacaoItemDto
    {
        public int Id { get; set; }
        public int CompetenciaId { get; set; }
        public string TituloCompetencia { get; set; } // ex: "Trabalho em Equipa"
        public string DescricaoCompetencia { get; set; }
        public int? NotaColaborador { get; set; }
        public int? NotaGestor { get; set; }
        public string? Comentario { get; set; }
    }
}
