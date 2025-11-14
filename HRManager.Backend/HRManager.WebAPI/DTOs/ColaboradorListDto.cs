namespace HRManager.WebAPI.DTOs
{
    public class ColaboradorListDto
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; }
        public string EmailPessoal { get; set; }
        public string NIF { get; set; }
        public string Cargo { get; set; }
        public string NomeInstituicao { get; set; } // Apenas o nome
        public bool IsAtivo { get; set; }
    }
}
