using HRManager.Application.Interfaces;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly HRManagerDbContext _context;
        private readonly ITenantService _tenantService;

        public NotificationService(HRManagerDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        public async Task NotifyUserByEmailAsync(string email, string titulo, string mensagem, string link = null)
        {
            // 1. Encontrar o User
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return; 

            // 2. Determinar o Tenant ID (Prioridade: User > Contexto Atual)
            // Usa o ID do user se existir, senão tenta o ID da sessão atual
            var tenantId = user.InstituicaoId ?? _tenantService.GetTenantId();

            // Se ainda assim for null (ex: admin global sem tenant a notificar outro admin), 
            // usamos Guid.Empty para não dar erro de SQL, ou uma lógica específica.
            // Aqui assumo que Guid.Empty é seguro ou que existe sempre um tenant.
            var finalTenantId = tenantId ?? Guid.Empty;

            var notif = new Notificacao
            {
                UserId = user.Id,
                Titulo = titulo,
                Mensagem = mensagem,
                Link = link,
                DataCriacao = DateTime.UtcNow,
                Lida = false,
                InstituicaoId = finalTenantId // 3. Preencher o campo novo
            };

            _context.Notificacoes.Add(notif);
            await _context.SaveChangesAsync();
        }

        public async Task NotifyManagersAsync(Guid? instituicaoId, string titulo, string mensagem, string link = null)
        {
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
            
            // Garantir que temos um ID válido para gravar na notificação. 
            // Se for para Master (instituicaoId null), usamos Guid.Empty ou o ID do próprio gestor se aplicável.
            var targetTenantId = instituicaoId ?? Guid.Empty; 

            var notificacoes = gestores.Select(g => new Notificacao
            {
                UserId = g.Id,
                Titulo = titulo,
                Mensagem = mensagem,
                Link = link,
                DataCriacao = DateTime.UtcNow,
                Lida = false,
                InstituicaoId = g.InstituicaoId ?? targetTenantId // Tenta usar o tenant do gestor
            });

            _context.Notificacoes.AddRange(notificacoes);
            await _context.SaveChangesAsync();
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
