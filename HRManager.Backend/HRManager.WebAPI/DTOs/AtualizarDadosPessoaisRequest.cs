using System.ComponentModel.DataAnnotations;

namespace HRManager.WebAPI.DTOs
{
    public class AtualizarDadosPessoaisRequest
    {
        [StringLength(200)]
        public string? Morada { get; set; }

        [StringLength(34)]
        public string? IBAN { get; set; }

        // No futuro, podemos adicionar "Contacto de Emergência", "Telefone", etc.
    }
}