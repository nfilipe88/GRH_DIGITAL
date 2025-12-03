using HRManager.WebAPI.Domain.enums;

namespace HRManager.WebAPI.DTOs
{
    public class AvaliacaoDto
    {
        public int Id { get; set; }
        public string NomeColaborador { get; set; }
        public string NomeGestor { get; set; }
        public string NomeCiclo { get; set; }
        public EstadoAvaliacao Estado { get; set; } // NaoIniciada, EmCurso, Concluida...
        public decimal? NotaFinal { get; set; }
        public DateTime? DataConclusao { get; set; }

        // Lista de Itens (Perguntas e Respostas)
        public List<AvaliacaoItemDto> Itens { get; set; } = new();
    }
}
