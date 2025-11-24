namespace HRManager.WebAPI.DTOs
{
    public class AusenciaSaldoDto
    {
        public string NomeColaborador { get; set; }
        public int SaldoFerias { get; set; }
        public int DiasPendentes { get; set; } // Útil para mostrar "X dias cativos"
    }
}
