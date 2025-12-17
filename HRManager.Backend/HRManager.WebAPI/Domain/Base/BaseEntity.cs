// Em HRManager.Domain/Entities/Base/BaseEntity.cs

using HRManager.WebAPI.Domain.Interfaces;

namespace HRManager.WebAPI.Domain.Base
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataAtualizacao { get; set; }
    }
}