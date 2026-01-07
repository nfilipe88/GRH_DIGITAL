namespace HRManager.WebAPI.DTOs
{
    public class PedidoDeclaracaoDto
    {
        public Guid Id { get; set; }
        public string NomeColaborador { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime DataSolicitacao { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string? CaminhoFicheiro { get; set; }
    }
}
