namespace HRManager.WebAPI.DTOs
{
    public class ItemRespostaDto
    {
        public Guid ItemId { get; set; }
        public int Nota { get; set; } // 1 a 5
        public string? Comentario { get; set; }
    }
}
