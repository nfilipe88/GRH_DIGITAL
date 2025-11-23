namespace HRManager.WebAPI.DTOs
{
    public class AusenciaDto
    {
        public int Id { get; set; }
        public string NomeColaborador { get; set; }
        public string Tipo { get; set; } // Texto (Férias, Doença...)
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int DiasTotal { get; set; } // Calculado
        public string Estado { get; set; } // Pendente, Aprovada...
        public DateTime DataSolicitacao { get; set; }
    }
}
