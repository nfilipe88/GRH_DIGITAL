using HRManager.WebAPI.DTOs;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IAusenciaService
    {
        Task<List<AusenciaDto>> GetAusenciasAsync(string userEmail, bool isGestorRH, bool isGestorMaster);
        Task<AusenciaSaldoDto> GetSaldoAsync(string userEmail);
        Task SolicitarAusenciaAsync(string userEmail, CriarAusenciaRequest request);
        // CORREÇÃO: Guid id
        Task ResponderAusenciaAsync(Guid id, ResponderAusenciaRequest request, string userEmail, bool isGestorRH);
        Task<byte[]> DownloadRelatorioExcelAsync(int mes, int ano);
    }
}
