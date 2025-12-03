using HRManager.WebAPI.DTOs;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IAusenciaService
    {
        Task<List<AusenciaDto>> GetAusenciasAsync(string userEmail, bool isGestorRH, bool isGestorMaster);
        Task<AusenciaSaldoDto> GetSaldoAsync(string userEmail);
        Task SolicitarAusenciaAsync(string userEmail, CriarAusenciaRequest request);
        Task ResponderAusenciaAsync(int id, ResponderAusenciaRequest request, string userEmail, bool isGestorRH);
        Task<byte[]> DownloadRelatorioExcelAsync(int mes, int ano); // Já deixamos preparado para o Excel
    }
}
