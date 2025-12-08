using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.Models
{
    public class Notificacao
    {
        [Key]
        public int Id { get; set; }

        // Quem recebe a notificação? (Ligação ao User de Login)
        [Required]
        public int UserId { get; set; }

        // Detalhes
        [Required]
        public string Titulo { get; set; } // Ex: "Pedido de Férias"

        [Required]
        public string Mensagem { get; set; } // Ex: "O João pediu 5 dias."

        public string? Link { get; set; } // Ex: "/gestao-ausencias" (Para onde ir ao clicar)

        public bool Lida { get; set; } = false;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public Guid InstituicaoId { get; internal set; }
    }
}
