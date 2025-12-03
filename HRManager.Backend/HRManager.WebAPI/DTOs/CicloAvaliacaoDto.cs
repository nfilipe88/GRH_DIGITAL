namespace HRManager.WebAPI.DTOs
{
    public class CicloAvaliacaoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public bool IsAtivo { get; set; }
    }
}
