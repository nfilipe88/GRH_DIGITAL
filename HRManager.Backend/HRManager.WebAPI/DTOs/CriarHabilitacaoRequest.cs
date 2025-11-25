using HRManager.WebAPI.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HRManager.WebAPI.DTOs
{
    public class CriarHabilitacaoRequest
    {
        [Required]
        public GrauAcademico Grau { get; set; }

        [Required]
        public string Curso { get; set; }

        [Required]
        public string InstituicaoEnsino { get; set; }

        [Required]
        public DateTime DataConclusao { get; set; }

        public IFormFile? Documento { get; set; } // O ficheiro PDF/Imagem
    }
}
