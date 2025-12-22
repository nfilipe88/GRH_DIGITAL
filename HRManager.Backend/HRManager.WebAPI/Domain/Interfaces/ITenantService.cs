// Em HRManager.Application/Interfaces/ITenantService.cs

namespace HRManager.Application.Interfaces
{
    public interface ITenantService
    {
        // Retorna o ID da instituição do utilizador atual.
        // Se for o Gestor Master (super-admin), pode retornar null ou um Guid especial.
        Guid? TenantId { get; }
        // Retorna o Gestor Master tem permissão de super-admin
        bool IsMasterTenant { get; }
        Guid GetInstituicaoId();
        Guid? GetTenantId();

    }
}