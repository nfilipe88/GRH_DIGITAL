namespace HRManager.WebAPI.Services
{
    public interface ITenantService
    {
        Guid GetInstituicaoId();
        bool IsMasterTenant { get; }
    }
}