using HRManager.WebAPI.Models;

namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface INotificationService
    {
        // Alterado para string? link = null
        Task NotifyUserByEmailAsync(string email, string titulo, string mensagem, string? link = null);

        Task NotifyManagersAsync(Guid instituicaoId, string titulo, string mensagem, string? link = null);

        // Este método não estava implementado no serviço, podes remover se não usares ou implementar
        // Task EnviarNotificacaoNovoPedido(PedidoDeclaracao pedido);
    }
}
