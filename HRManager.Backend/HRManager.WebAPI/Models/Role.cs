using HRManager.WebAPI.Domain.Base;

namespace HRManager.WebAPI.Models
{
    public class Role : TenantEntity
    {
        public string Name { get; set; } = string.Empty; // Ex: "GestorRH"
        public string Description { get; set; } = string.Empty; // Ex: "Acesso total ao módulo de RH"
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
