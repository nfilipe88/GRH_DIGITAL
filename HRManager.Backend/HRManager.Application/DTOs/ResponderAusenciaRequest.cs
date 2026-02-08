using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class ResponderAusenciaRequest
    {
        [Required]
        public bool Aprovado { get; set; } // true = Aprovada, false = Rejeitada

        [MaxLength(500)]
        public string? Comentario { get; set; } // Obrigatório se rejeitado (validaremos no controller)
    }
}
