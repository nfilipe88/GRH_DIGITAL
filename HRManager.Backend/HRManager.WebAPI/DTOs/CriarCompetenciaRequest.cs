using HRManager.WebAPI.Domain.enums;
using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class CriarCompetenciaRequest
    {
        [Required]
        public string Nome { get; set; } = string.Empty; // Ex: "Trabalho em Equipa"

        public string Descricao { get; set; } = string.Empty;

        [Required]
        public TipoCompetencia Tipo { get; set; } // 0 = Comportamental, 1 = Técnica
    }
}
