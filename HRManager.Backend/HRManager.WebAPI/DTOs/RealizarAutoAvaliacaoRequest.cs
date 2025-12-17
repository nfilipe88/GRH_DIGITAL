namespace HRManager.WebAPI.DTOs
{
    // DTO para o Colaborador
    public class RealizarAutoAvaliacaoRequest
    {
        public List<ItemRespostaDto> Respostas { get; set; } = new();
        public bool Finalizar { get; set; } // Se true, envia para o gestor. Se false, é rascunho.
    }
}
