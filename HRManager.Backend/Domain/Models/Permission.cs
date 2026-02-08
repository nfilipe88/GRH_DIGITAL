namespace HRManager.WebAPI.Models
{
    public class Permission
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty; // Ex: "USERS_VIEW", "INSTITUTIONS_CREATE"
        public string Name { get; set; } = string.Empty; // Ex: "Ver Utilizadores"
        public string Module { get; set; } = string.Empty; // Ex: "Gestão", "RH", "Avaliações"
        public string Category { get; set; } = string.Empty; // Ex: "Leitura", "Escrita", "Administração"
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation - Inicialize a coleção
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
