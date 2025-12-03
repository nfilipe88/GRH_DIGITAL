namespace HRManager.WebAPI.DTOs
{
    public class UserDetailsDto
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string Cargo { get; set; }

        public bool IsActive { get; set; }
        public string? InstituicaoNome { get; set; } // Útil para mostrar no menu "Logado como..."}
    }
}
