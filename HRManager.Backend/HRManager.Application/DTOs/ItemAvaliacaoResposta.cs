namespace HRManager.WebAPI.DTOs
{
    public class ItemAvaliacaoResposta
    {
        public int ItemId { get; set; }
        public int Nota { get; set; } // 1 a 5
        public string? Comentario { get; set; }
    }
}
