using HRManager.WebAPI.Domain.Base;
using Microsoft.AspNetCore.Identity;

namespace HRManager.WebAPI.Models
{
    public class Role : IdentityRole<Guid>
    {
        public string Description { get; set; } = string.Empty;
        public bool IsSystemRole { get; set; } = false; // Não pode ser editado (ex: GestorMaster)
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? InstituicaoId { get; set; } // Se null, role global; se preenchido, role específica da instituição

        // Navigation
        public Instituicao? Instituicao { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
