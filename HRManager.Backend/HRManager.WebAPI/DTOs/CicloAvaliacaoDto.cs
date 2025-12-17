namespace HRManager.WebAPI.DTOs
{
    public class CicloAvaliacaoDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public bool IsAtivo { get; set; }
    }
}
