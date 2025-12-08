using HRManager.Application.Interfaces;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace HRManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DeclaracoesController : ControllerBase
    {
        private readonly HRManagerDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public DeclaracoesController(HRManagerDbContext context, ITenantService tenantService, IWebHostEnvironment env, IEmailService emailService, INotificationService notificationService)
        {
            _context = context;
            _tenantService = tenantService;
            _env = env;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        // ---
        // GET: Listar Pedidos (Lógica Multi-Tenant)
        // ---
        [HttpGet]
        public async Task<IActionResult> GetPedidos()
        {
            var query = _context.PedidosDeclaracao.AsQueryable();

            // Filtros de Segurança
            if (User.IsInRole("GestorRH"))
            {
                var tenantId = _tenantService.GetTenantId();
                if (tenantId.HasValue)
                    query = query.Where(p => p.Colaborador.InstituicaoId == tenantId.Value);
            }
            else if (!User.IsInRole("GestorMaster")) // É Colaborador
            {
                var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
                query = query.Where(p => p.Colaborador.EmailPessoal == email);
            }

            var lista = await query
                .Include(p => p.Colaborador)
                .OrderByDescending(p => p.DataSolicitacao)
                .Select(p => new PedidoDeclaracaoDto
                {
                    Id = p.Id,
                    NomeColaborador = p.Colaborador.NomeCompleto,
                    Tipo = p.Tipo.ToString(),
                    Estado = p.Estado.ToString(),
                    DataSolicitacao = p.DataSolicitacao,
                    DataConclusao = p.DataConclusao,
                    CaminhoFicheiro = p.CaminhoFicheiro
                })
                .ToListAsync();

            return Ok(lista);
        }

        // ---
        // POST: Solicitar Declaração (Colaborador)
        // ---
        [HttpPost]
        public async Task<IActionResult> Solicitar([FromBody] CriarPedidoDeclaracaoRequest request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            var colaborador = await _context.Colaboradores.FirstOrDefaultAsync(c => c.EmailPessoal == email);

            if (colaborador == null) return BadRequest(new { message = "Colaborador não encontrado." });

            // Validar se já tem um pendente do mesmo tipo (para evitar spam)
            bool existePendente = await _context.PedidosDeclaracao
                .AnyAsync(p => p.ColaboradorId == colaborador.Id
                               && p.Tipo == request.Tipo
                               && p.Estado == EstadoPedidoDeclaracao.Pendente);

            if (existePendente) return BadRequest(new { message = "Já tem um pedido pendente para este tipo de declaração." });

            var pedido = new PedidoDeclaracao
            {
                ColaboradorId = colaborador.Id,
                Tipo = request.Tipo,
                Observacoes = request.Observacoes,
                Estado = EstadoPedidoDeclaracao.Pendente,
                DataSolicitacao = DateTime.UtcNow
            };

            _context.PedidosDeclaracao.Add(pedido);
            await _context.SaveChangesAsync();

            // (Opcional) Enviar notificação ao RH a avisar do novo pedido?
            await _notificationService.EnviarNotificacaoNovoPedido(pedido);

            return Ok(new { message = "Pedido de declaração enviado." });
        }

        // ---
        // PUT: Resolver Pedido (Gestor faz upload do PDF final)
        // ---
        [HttpPut("{id}/resolver")]
        [Authorize(Roles = "GestorMaster, GestorRH")]
        public async Task<IActionResult> Resolver(int id, [FromForm] IFormFile? documento, [FromQuery] bool rejeitar = false)
        {
            var pedido = await _context.PedidosDeclaracao
                .Include(p => p.Colaborador)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            // Validação Multi-tenant
            if (User.IsInRole("GestorRH"))
            {
                var tenantId = _tenantService.GetTenantId();
                if (pedido.Colaborador.InstituicaoId != tenantId) return Forbid();
            }

            // --- CENÁRIO 1: REJEITAR ---
            if (rejeitar)
            {
                pedido.Estado = EstadoPedidoDeclaracao.Rejeitado;
                pedido.DataConclusao = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                // Enviar Email de Rejeição
                await EnviarEmailNotificacao(pedido, false);
                return Ok(new { message = "Pedido rejeitado." });
            }

            // --- CENÁRIO 2: APROVAR (Upload Obrigatório) ---
            if (documento == null || documento.Length == 0)
            {
                return BadRequest(new { message = "É obrigatório carregar o documento para concluir o pedido." });
            }

            // Upload
            var pasta = Path.Combine(_env.WebRootPath, "uploads", "declaracoes");
            if (!Directory.Exists(pasta)) Directory.CreateDirectory(pasta);

            var nomeFicheiro = Guid.NewGuid() + Path.GetExtension(documento.FileName);
            var caminho = Path.Combine(pasta, nomeFicheiro);

            using (var stream = new FileStream(caminho, FileMode.Create))
            {
                await documento.CopyToAsync(stream);
            }

            pedido.CaminhoFicheiro = $"uploads/declaracoes/{nomeFicheiro}";
            pedido.Estado = EstadoPedidoDeclaracao.Concluido;
            pedido.DataConclusao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Enviar Email de Sucesso com link ou aviso
            // await EnviarEmailNotificacao(pedido, true);
            // Enviar Email
            await EnviarEmailNotificacao(pedido, !rejeitar);

            // --- ADICIONAR ISTO (SINO) ---
            string mensagem = rejeitar
                ? $"O seu pedido de declaração ({pedido.Tipo}) foi rejeitado."
                : $"A sua declaração ({pedido.Tipo}) está pronta para download.";

            await _notificationService.NotifyUserByEmailAsync(
                pedido.Colaborador.EmailPessoal,
                rejeitar ? "Pedido Rejeitado" : "Declaração Pronta",
                mensagem,
                "/minhas-declaracoes" // Link para onde o user vai ao clicar
            );
            // -----------------------------

            return Ok(new { message = "Declaração emitida e enviada ao colaborador." });
        }

        private async Task EnviarEmailNotificacao(PedidoDeclaracao pedido, bool sucesso)
        {
            try
            {
                string assunto = sucesso ? "Declaração Emitida ✅" : "Pedido de Declaração Rejeitado ❌";
                var sb = new StringBuilder();
                sb.AppendLine($"<h3>Olá, {pedido.Colaborador.NomeCompleto}</h3>");

                if (sucesso)
                {
                    sb.AppendLine($"<p>A sua declaração para <strong>{pedido.Tipo}</strong> já está disponível.</p>");
                    sb.AppendLine("<p>Aceda ao portal para descarregar o documento.</p>");
                }
                else
                {
                    sb.AppendLine($"<p>O seu pedido de declaração para <strong>{pedido.Tipo}</strong> foi rejeitado pelo RH.</p>");
                }

                await _emailService.SendEmailAsync(pedido.Colaborador.EmailPessoal, assunto, sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO CRÍTICO EMAIL: {ex.Message}");
            }
        }
    }
}
