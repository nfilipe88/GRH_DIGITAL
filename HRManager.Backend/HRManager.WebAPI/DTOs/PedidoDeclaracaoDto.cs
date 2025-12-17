namespace HRManager.WebAPI.DTOs
{
    public class PedidoDeclaracaoDto
    {
        public Guid Id { get; set; }
        public string NomeColaborador { get; set; }
        public string Tipo { get; set; }
        public string Estado { get; set; }
        public DateTime DataSolicitacao { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string? CaminhoFicheiro { get; set; }
    }
}
