using HRManager.WebAPI.Domain.Interfaces;

namespace HRManager.WebAPI.Domain.Base
{
    public abstract class TenantEntity : BaseEntity, IHaveTenant
    {
        public Guid InstituicaoId { get; set; }
    }
}
