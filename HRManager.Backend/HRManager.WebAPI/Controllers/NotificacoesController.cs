using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificacoesController : ControllerBase
    {
        private readonly HRManagerDbContext _context;

        public NotificacoesController(HRManagerDbContext context)
        {
            _context = context;
        }

        // GET: Minhas notificações não lidas (ou recentes)
        [HttpGet]
        public async Task<IActionResult> GetMinhasNotificacoes()
        {
            // Descobrir o ID do utilizador logado através do token
            // (O TokenService guarda o Id como ClaimTypes.NameIdentifier)
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            Guid userId = Guid.Parse(userIdStr);

            var notificacoes = await _context.Notificacoes
                .Where(n => n.UserId == userId && !n.Lida)
                .OrderByDescending(n => n.DataCriacao)
                .Take(10) // Limitar às 10 mais recentes
                .ToListAsync();

            return Ok(notificacoes);
        }

        // PUT: Marcar como lida
        [HttpPut("{id}/ler")]
        public async Task<IActionResult> MarcarComoLida(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid userId = Guid.Parse(userIdStr);

            var notif = await _context.Notificacoes.FindAsync(id);

            // Segurança: Só posso marcar como lida a MINHA notificação
            if (notif == null || notif.UserId != userId) return NotFound();

            notif.Lida = true;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
