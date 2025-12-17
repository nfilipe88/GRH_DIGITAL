using HRManager.WebAPI.Domain.enums;
using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class CriarCompetenciaRequest
    {
        [Required]
        public string Nome { get; set; } // Ex: "Trabalho em Equipa"

        public string? Descricao { get; set; }

        [Required]
        public TipoCompetencia Tipo { get; set; } // 0 = Comportamental, 1 = Técnica
    }
}
