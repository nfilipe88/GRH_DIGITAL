namespace HRManager.WebAPI.Domain.Interfaces
{
    public interface INotificationService
    {
        // Envia para UM utilizador específico (pelo Email)
        Task NotifyUserByEmailAsync(string email, string titulo, string mensagem, string link = null);

        // Envia para TODOS os Gestores de uma Instituição (ex: quando alguém pede férias)
        Task NotifyManagersAsync(Guid? instituicaoId, string titulo, string mensagem, string link = null);
    }
}
