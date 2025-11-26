namespace HRManager.WebAPI.DTOs
{
    public class UserDetailsDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string? NomeInstituicao { get; set; } // Útil para mostrar no menu "Logado como..."}
    }
}
