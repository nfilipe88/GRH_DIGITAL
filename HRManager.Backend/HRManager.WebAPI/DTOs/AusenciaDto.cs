namespace HRManager.WebAPI.DTOs
{
    public class AusenciaDto
    {
        public Guid Id { get; set; }
        public string NomeColaborador { get; set; } = String.Empty;
        public string Tipo { get; set; }= String.Empty; // Texto (Férias, Doença...)
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int DiasTotal { get; set; } // Calculado
        public string Estado { get; set; }= String.Empty; // Pendente, Aprovada...
        public DateTime DataSolicitacao { get; set; }
        // *** CAMINHO DOS DOCUMENTOS CARREGADOS ***
        public string? CaminhoDocumento { get; set; }
    }
}
