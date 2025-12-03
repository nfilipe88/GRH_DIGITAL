// Em HRManager.Domain/Entities/Base/BaseEntity.cs

using HRManager.WebAPI.Domain.Interfaces;

namespace HRManager.WebAPI.Domain.Base
{
    public abstract class BaseEntity : IHaveTenant
    {
        public Guid Id { get; set; }
        
        // Propriedade herdada da interface
        public Guid InstituicaoId { get; set; } 
    }
}