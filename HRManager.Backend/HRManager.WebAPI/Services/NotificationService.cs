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

        public async Task NotifyUserByEmailAsync(string email, string titulo, string mensagem, string link = null)
        {
            // 1. Encontrar o User ID com base no email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return; // Se o utilizador não tiver login, não recebe notificação no sino

            // 2. Criar notificação
            var notif = new Notificacao
            {
                UserId = user.Id,
                Titulo = titulo,
                Mensagem = mensagem,
                Link = link,
                DataCriacao = DateTime.UtcNow,
                Lida = false
            };

            _context.Notificacoes.Add(notif);
            await _context.SaveChangesAsync();
        }

        public async Task NotifyManagersAsync(Guid? instituicaoId, string titulo, string mensagem, string link = null)
        {
            // 1. Encontrar todos os Gestores relevantes
            // - Se instituicaoId for null, notifica APENAS Gestores Master
            // - Se tiver ID, notifica GestoresRH dessa empresa E Gestores Master

            var query = _context.Users.AsQueryable();

            if (instituicaoId.HasValue)
            {
                query = query.Where(u =>
                    (u.Role == "GestorRH" && u.InstituicaoId == instituicaoId.Value) ||
                    u.Role == "GestorMaster");
            }
            else
            {
                query = query.Where(u => u.Role == "GestorMaster");
            }

            var gestores = await query.ToListAsync();

            // 2. Criar uma notificação para cada gestor
            var notificacoes = gestores.Select(g => new Notificacao
            {
                UserId = g.Id,
                Titulo = titulo,
                Mensagem = mensagem,
                Link = link,
                DataCriacao = DateTime.UtcNow,
                Lida = false
            });

            _context.Notificacoes.AddRange(notificacoes);
            await _context.SaveChangesAsync();
        }
    }
}
