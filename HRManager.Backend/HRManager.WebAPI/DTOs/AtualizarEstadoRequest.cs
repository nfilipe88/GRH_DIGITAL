using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class AtualizarEstadoRequest
    {
        [Required]
        public bool IsAtiva { get; set; }
    }
}
