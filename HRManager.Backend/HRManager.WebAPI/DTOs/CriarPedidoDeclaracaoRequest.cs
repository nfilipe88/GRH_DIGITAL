using HRManager.WebAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class CriarPedidoDeclaracaoRequest
    {
        [Required]
        public TipoDeclaracao Tipo { get; set; }

        public string? Observacoes { get; set; }
    }
}
