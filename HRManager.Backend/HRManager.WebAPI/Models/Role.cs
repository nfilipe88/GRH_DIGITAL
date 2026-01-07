using HRManager.WebAPI.Domain.Base;
using Microsoft.AspNetCore.Identity;

namespace HRManager.WebAPI.Models
{
    public class Role : IdentityRole<Guid>
    {
        //public string Name { get; set; } = string.Empty; // Ex: "GestorRH"
        public string Description { get; set; } = string.Empty; // Ex: "Acesso total ao módulo de RH"
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
