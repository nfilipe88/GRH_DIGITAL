using HRManager.WebAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class CriarAusenciaRequest
    {
        [Required]
        public TipoAusencia Tipo { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        [Required]
        public DateTime DataFim { get; set; }

        [MaxLength(500)]
        public string? Motivo { get; set; }
    }
}
