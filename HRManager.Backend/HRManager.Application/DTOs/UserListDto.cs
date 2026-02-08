namespace HRManager.WebAPI.DTOs
{
    public class UserListDto
    {
        public Guid Id { get; set; }
        public string NomeCompleto { get; set; }=string.Empty;
        public string Email { get; set; }=string.Empty;
        public string Role { get; set; }=string.Empty;
        public string NomeInstituicao { get; set; }=string.Empty; // O nome, não o ID
        public string InstituicaoNome { get; set; } =string.Empty;
        public bool IsAtivo { get; set; }
    }
}
