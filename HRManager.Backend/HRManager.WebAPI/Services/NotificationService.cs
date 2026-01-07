using HRManager.Application.Interfaces;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly HRManagerDbContext _context;

        public NotificationService(HRManagerDbContext context)
        {
            _context = context;
        }

        public async Task NotifyUserByEmailAsync(string email, string titulo, string mensagem, string? link = null)
        {
            if (string.IsNullOrEmpty(email)) return;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return;

            var notif = new Notificacao
            {
                UserId = user.Id,
                Titulo = titulo,
                Mensagem = mensagem,
                Link = link,
                DataCriacao = DateTime.UtcNow,
                Lida = false,
                InstituicaoId = user.InstituicaoId // Guid
            };

            _context.Notificacoes.Add(notif);
            await _context.SaveChangesAsync();
        }

        public async Task NotifyManagersAsync(Guid instituicaoId, string titulo, string mensagem, string? link = null)
        {
            // Buscar Users que tenham a Role "Gestor" E pertençam à Instituição
            var gestores = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.InstituicaoId == instituicaoId && 
                            u.UserRoles.Any(ur => ur.Role.Name == "Gestor"))
                .ToListAsync();

            var notifs = gestores.Select(g => new Notificacao
            {
                UserId = g.Id,
                Titulo = titulo,
                Mensagem = mensagem,
                Link = link,
                DataCriacao = DateTime.UtcNow,
                Lida = false,
                InstituicaoId = instituicaoId
            }).ToList();

            if (notifs.Any())
            {
                _context.Notificacoes.AddRange(notifs);
                await _context.SaveChangesAsync();
            }
        }
        
        public Task EnviarNotificacaoNovoPedido(PedidoDeclaracao pedido)
        {
            var titulo = "Novo Pedido de Declaração";
            var mensagem = $"O colaborador {pedido.Colaborador.NomeCompleto} solicitou uma nova declaração.";
            var link = $"/declaracoes/{pedido.Id}";

            return NotifyManagersAsync(pedido.Colaborador.InstituicaoId, titulo, mensagem, link);
        }
    }
}
