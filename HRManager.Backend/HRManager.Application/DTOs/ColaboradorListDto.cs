using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class ColaboradorListDto
    {
        public Guid Id { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        // O Serviço estava a chamar 'Email', mas aqui devia ser explícito
        public string Email { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        // O Serviço tenta preencher 'Status'
        public bool IsAtivo { get; set; }
        // Campos opcionais se necessários
        public string? NomeInstituicao { get; set; }
    }
}
