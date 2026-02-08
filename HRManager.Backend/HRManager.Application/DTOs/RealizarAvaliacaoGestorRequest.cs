namespace HRManager.WebAPI.DTOs
{
    // DTO para o Gestor (já tens algo parecido, vamos ajustar)
    public class RealizarAvaliacaoGestorRequest
    {
        public List<ItemRespostaDto> Respostas { get; set; } = new();
        public string? ComentarioFinal { get; set; }
        public bool Finalizar { get; set; }
    }
}
