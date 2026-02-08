using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IAvaliacaoService
    {
        Task<List<CicloAvaliacaoDto>> GetCiclosAsync();
        // Processo de Avaliação
        Task<List<AvaliacaoDto>> GetMinhasAvaliacoesAsync(string emailColaborador); // Para o Colaborador
        Task<List<AvaliacaoDto>> GetAvaliacoesEquipaAsync(string emailGestor); // Para o Gestor
        Task<AvaliacaoDto> GetAvaliacaoPorIdAsync(Guid id, string emailSolicitante);
        Task<AvaliacaoDto> IniciarAvaliacaoAsync(Guid colaboradorId, Guid cicloId, string emailGestor);
        Task<AvaliacaoDto> RealizarAutoAvaliacaoAsync(Guid avaliacaoId, RealizarAutoAvaliacaoRequest request, string emailColaborador);
        Task<AvaliacaoDto> RealizarAvaliacaoGestorAsync(Guid avaliacaoId, RealizarAvaliacaoGestorRequest request, string emailGestor);
        // NOVOS MÉTODOS DE CONFIGURAÇÃO (FASE 1)
        Task<Competencia> CriarCompetenciaAsync(CriarCompetenciaRequest request);
        Task<List<Competencia>> GetCompetenciasAsync(); // Listar todas as disponíveis
        Task<bool> ToggleCompetenciaStatusAsync(Guid id); // Ativar/Desativar uma pergunta
        Task<CicloAvaliacaoDto> CriarCicloAsync(CriarCicloRequest request);
        // Estatísticas
        Task<decimal> CalcularMediaFinalAsync(Guid avaliacaoId);
    }
}
