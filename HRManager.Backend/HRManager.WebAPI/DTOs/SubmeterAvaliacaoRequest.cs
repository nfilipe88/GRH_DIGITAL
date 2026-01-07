namespace HRManager.WebAPI.DTOs
{
    public class SubmeterAvaliacaoRequest
    {
        public List<ItemAvaliacaoResposta>? Respostas { get; set; }
        public string? ComentarioFinal { get; set; }
        public bool Finalizar { get; set; } // Se true, fecha a avaliação. Se false, apenas guarda rascunho.
    }
}
