using Microsoft.AspNetCore.Identity;

namespace HRManager.WebAPI.Models
{
    public class UserRole : IdentityUserRole<Guid>
    {
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public Guid? AssignedBy { get; set; } // Quem atribuiu
        public virtual User User { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
}