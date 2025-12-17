using System;
using System.ComponentModel.DataAnnotations;
using HRManager.WebAPI.Domain.Base;

namespace HRManager.WebAPI.Models
{
    public class Notificacao : TenantEntity
    {

        [Required]
        public Guid UserId { get; set; }
        public User? User { get; set; }
        [Required]
        public string Titulo { get; set; } = string.Empty;
        [Required]
        public string Mensagem { get; set; } = string.Empty;
        public string? Link { get; set; }
        public bool Lida { get; set; } = false;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
