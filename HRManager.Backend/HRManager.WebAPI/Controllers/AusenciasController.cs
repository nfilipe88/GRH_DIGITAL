using HRManager.Application.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRManager.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requer login para tudo
    public class AusenciasController : ControllerBase
    {
        private readonly HRManagerDbContext _context;
        private readonly ITenantService _tenantService;

        public AusenciasController(HRManagerDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        // ---
        // GET: Listar Ausências (Inteligente)
        // ---
        [HttpGet]
        public async Task<IActionResult> GetAusencias()
        {
            var query = _context.Ausencias.AsQueryable();

            // 1. Se for Colaborador, vê SÓ as suas
            if (User.IsInRole("Colaborador")) // Vamos ter de criar este Role no futuro
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                query = query.Where(a => a.Colaborador.EmailPessoal == email);
            }
            // 2. Se for GestorRH, vê da sua instituição
            else if (User.IsInRole("GestorRH"))
            {
                var tenantId = _tenantService.GetTenantId();
                if (tenantId.HasValue)
                {
                    query = query.Where(a => a.Colaborador.InstituicaoId == tenantId.Value);
                }
            }
            // 3. Se for GestorMaster, vê tudo (ou pode filtrar, implementaremos depois)

            var lista = await query
                .Select(a => new AusenciaDto
                {
                    Id = a.Id,
                    NomeColaborador = a.Colaborador.NomeCompleto,
                    Tipo = a.Tipo.ToString(),
                    DataInicio = a.DataInicio,
                    DataFim = a.DataFim,
                    DiasTotal = (a.DataFim - a.DataInicio).Days + 1, // Cálculo simples
                    Estado = a.Estado.ToString(),
                    DataSolicitacao = a.DataSolicitacao
                })
                .OrderByDescending(a => a.DataSolicitacao)
                .ToListAsync();

            return Ok(lista);
        }

        // ---
        // POST: Solicitar Ausência (Só para Colaboradores)
        // ---
        [HttpPost]
        // [Authorize(Roles = "Colaborador")] // Descomentar quando tivermos o Role
        public async Task<IActionResult> SolicitarAusencia([FromBody] CriarAusenciaRequest request)
        {
            // 1. Descobrir quem é o colaborador logado (pelo email do token)
            //var email = User.FindFirstValue(ClaimTypes.Email);
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            var colaborador = await _context.Colaboradores.FirstOrDefaultAsync(c => c.EmailPessoal == email);

            if (colaborador == null)
            {
                return BadRequest(new { message = "Não foi encontrado um perfil de colaborador associado a este utilizador." });
            }

            // Use esta abordagem mais segura que procura por ambos os nomes:

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(new { message = "Token inválido: Email não encontrado." });
            }

            // 2. Validar datas
            if (request.DataInicio > request.DataFim)
            {
                return BadRequest(new { message = "A data de fim deve ser posterior à data de início." });
            }

            // 3. Criar o pedido
            var novaAusencia = new Ausencia
            {
                ColaboradorId = colaborador.Id,
                Tipo = request.Tipo,
                DataInicio = DateTime.SpecifyKind(request.DataInicio, DateTimeKind.Utc),
                DataFim = DateTime.SpecifyKind(request.DataFim, DateTimeKind.Utc),
                Motivo = request.Motivo,
                Estado = EstadoAusencia.Pendente,
                DataSolicitacao = DateTime.UtcNow
            };

            _context.Ausencias.Add(novaAusencia);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { message = "Pedido de ausência submetido com sucesso." });
        }

        // ---
        // PUT: Aprovar/Rejeitar (Só para Gestores)
        // ---
        [HttpPut("{id}/responder")]
        [Authorize(Roles = "GestorMaster, GestorRH")]
        public async Task<IActionResult> ResponderAusencia(int id, [FromBody] ResponderAusenciaRequest request)
        {
            var ausencia = await _context.Ausencias
                                         .Include(a => a.Colaborador)
                                         .FirstOrDefaultAsync(a => a.Id == id);

            if (ausencia == null) return NotFound(new { message = "Pedido não encontrado." });

            // Segurança: GestorRH só mexe na sua instituição
            if (User.IsInRole("GestorRH"))
            {
                var tenantId = _tenantService.GetTenantId();
                if (ausencia.Colaborador.InstituicaoId != tenantId)
                {
                    return Forbid();
                }
            }

            // Validação: Rejeição exige comentário
            if (!request.Aprovado && string.IsNullOrWhiteSpace(request.Comentario))
            {
                return BadRequest(new { message = "É obrigatório fornecer uma justificativa ao rejeitar." });
            }

            // Atualizar estado
            ausencia.Estado = request.Aprovado ? EstadoAusencia.Aprovada : EstadoAusencia.Rejeitada;
            ausencia.ComentarioGestor = request.Comentario;
            ausencia.DataResposta = DateTime.UtcNow;

            // (Futuro: Aqui descontaríamos do saldo de férias se Aprovado)

            await _context.SaveChangesAsync();

            string acao = request.Aprovado ? "aprovado" : "rejeitado";
            return Ok(new { message = $"Pedido {acao} com sucesso." });
        }
    }
}
