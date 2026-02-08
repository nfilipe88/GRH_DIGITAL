using HRManager.WebAPI.Domain.Base;
using HRManager.WebAPI.Domain.enums;

namespace HRManager.WebAPI.Models
{
    public class AuditLog : BaseEntity
    {
        //public Guid Id { get; set; }
        public EntityType EntityType { get; set; }
        public Guid EntityId { get; set; }
        public AuditAction Action { get; set; }
        public string? OldValues { get; set; } // JSON com valores antigos
        public string? NewValues { get; set; } // JSON com valores novos
        public string EntityName { get; set; } = string.Empty; // Qual tabela?
        public string? Changes { get; set; } // Descrição das mudanças
        public Guid UserId { get; set; } // Quem fez a alteração
        public string UserIp { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
        public Guid InstituicaoId { get; set; }
    }
}
