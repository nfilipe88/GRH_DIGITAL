using HRManager.WebAPI.DTOs;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface IDeclaracaoService
    {
        // Colaborador solicita
        Task<PedidoDeclaracaoDto> CriarPedidoAsync(CriarPedidoDeclaracaoRequest request, string emailColaborador);
        Task<List<PedidoDeclaracaoDto>> GetMeusPedidosAsync(string emailColaborador);
        Task<List<PedidoDeclaracaoDto>> GetPedidosPendentesAsync(); // Para o RH
        // Processamento (RH)
        Task<byte[]> GerarDeclaracaoPdfAsync(Guid idPedido, string emailGestor);
        Task AtualizarEstadoPedidoAsync(Guid idPedido, bool aprovado, string emailGestor);
    }
}
