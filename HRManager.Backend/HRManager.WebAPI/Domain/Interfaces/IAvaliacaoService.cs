using HRManager.WebAPI.DTOs;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IAvaliacaoService
    {
        // Gestão de Ciclos (ex: "Avaliação 2025")
        Task<List<CicloAvaliacaoDto>> GetCiclosAsync();
        Task<CicloAvaliacaoDto> CriarCicloAsync(CriarCicloRequest request);

        // Processo de Avaliação
        Task<List<AvaliacaoDto>> GetMinhasAvaliacoesAsync(string emailColaborador); // Para o Colaborador
        Task<List<AvaliacaoDto>> GetAvaliacoesEquipaAsync(string emailGestor); // Para o Gestor

        Task<AvaliacaoDto> IniciarAvaliacaoAsync(int colaboradorId, int cicloId, string emailGestor);
        Task<AvaliacaoDto> SubmeterAvaliacaoAsync(int avaliacaoId, SubmeterAvaliacaoRequest request, string emailAvaliador);

        // Estatísticas
        Task<decimal> CalcularMediaFinalAsync(int avaliacaoId);
    }
}
