namespace HRManager.WebAPI.DTOs
{
    public class UserListDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }=String.Empty;
        public string Email { get; set; }=String.Empty;
        public string Role { get; set; }=String.Empty;
        public string NomeInstituicao { get; set; }=String.Empty; // O nome, não o ID
    }
}
