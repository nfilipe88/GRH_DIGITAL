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
            // 1. Identificar o colaborador
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            var colaborador = await _context.Colaboradores.FirstOrDefaultAsync(c => c.EmailPessoal == email);

            if (colaborador == null) return BadRequest(new { message = "Colaborador não encontrado." });

            // 2. Validar Datas Básicas
            if (request.DataInicio > request.DataFim)
            {
                return BadRequest(new { message = "A data de fim deve ser posterior à data de início." });
            }
            if (request.DataInicio < DateTime.Today)
            {
                return BadRequest(new { message = "Não pode solicitar ausências no passado." });
            }

            // --- NOVA VALIDAÇÃO 1: Conflito de Datas ---
            // Verifica se já existe alguma ausência (Pendente ou Aprovada) que se sobreponha
            bool existeConflito = await _context.Ausencias
                .AnyAsync(a => a.ColaboradorId == colaborador.Id
                                && a.Estado != EstadoAusencia.Rejeitada
                                && a.Estado != EstadoAusencia.Cancelada
                                && a.DataInicio <= request.DataFim
                                && a.DataFim >= request.DataInicio);

            if (existeConflito)
            {
                return BadRequest(new { message = "Já existe uma solicitação para este período." });
            }

            // --- NOVA VALIDAÇÃO 2: Verificar Saldo ---
            // Apenas para férias, calculamos os dias
            // (Nota: Para ser perfeito, deveríamos excluir fins de semana, mas para o MVP usamos dias corridos ou totais)
            if (request.Tipo == TipoAusencia.Ferias)
            {
                int diasSolicitados = (request.DataFim - request.DataInicio).Days + 1;

                // Vamos também contar quantos dias já tem "Pendentes" para não deixar pedir 22 dias duas vezes
                int diasJaPendentes = await _context.Ausencias
                    .Where(a => a.ColaboradorId == colaborador.Id
                                && a.Tipo == TipoAusencia.Ferias
                                && a.Estado == EstadoAusencia.Pendente)
                    .SumAsync(a => (a.DataFim - a.DataInicio).Days + 1);

                if (colaborador.SaldoFerias < (diasSolicitados + diasJaPendentes))
                {
                    return BadRequest(new { message = $"Saldo insuficiente. Tem {colaborador.SaldoFerias} dias disponíveis (e {diasJaPendentes} já pendentes)." });
                }
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

            return StatusCode(201, new { message = "Pedido submetido com sucesso." });
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

            // (Lógica de segurança do GestorRH mantém-se igual...)
            if (User.IsInRole("GestorRH"))
            {
                var tenantId = _tenantService.GetTenantId();
                if (ausencia.Colaborador.InstituicaoId != tenantId) return Forbid();
            }

            // Evitar aprovar/rejeitar coisas já processadas
            if (ausencia.Estado != EstadoAusencia.Pendente)
            {
                return BadRequest(new { message = "Este pedido já foi processado." });
            }

            // --- LÓGICA DE SALDO NA APROVAÇÃO ---
            if (request.Aprovado)
            {
                if (ausencia.Tipo == TipoAusencia.Ferias)
                {
                    int dias = (ausencia.DataFim - ausencia.DataInicio).Days + 1;

                    // Validar saldo novamente (caso tenha mudado entretanto)
                    if (ausencia.Colaborador.SaldoFerias < dias)
                    {
                        return BadRequest(new { message = "Não é possível aprovar: O colaborador já não tem saldo suficiente." });
                    }

                    // Descontar do saldo
                    ausencia.Colaborador.SaldoFerias -= dias;
                    // O EF Core é inteligente: ao mudar a propriedade do Colaborador, ele vai gerar o UPDATE na tabela Colaboradores também
                }

                ausencia.Estado = EstadoAusencia.Aprovada;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.Comentario))
                    return BadRequest(new { message = "É obrigatório fornecer uma justificativa ao rejeitar." });

                ausencia.Estado = EstadoAusencia.Rejeitada;
            }

            ausencia.ComentarioGestor = request.Comentario;
            ausencia.DataResposta = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            string acao = request.Aprovado ? "aprovado" : "rejeitado";
            return Ok(new { message = $"Pedido {acao} com sucesso." });
        }

        [HttpGet("saldo")]
        public async Task<IActionResult> GetMeuSaldo()
        {
            // 1. Identificar quem está logado
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var collaborator = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.EmailPessoal == email);

            if (collaborator == null) return NotFound(new { message = "Colaborador não encontrado." });

            // 2. Calcular dias "em análise" (que ainda não foram descontados, mas estão pedidos)
            //    Assumimos apenas o tipo 'Ferias' consome saldo
            var diasPendentes = await _context.Ausencias
                .Where(a => a.ColaboradorId == collaborator.Id
                            && a.Tipo == TipoAusencia.Ferias
                            && a.Estado == EstadoAusencia.Pendente)
                .SumAsync(a => (a.DataFim - a.DataInicio).Days + 1);

            // 3. Retornar DTO
            var saldoDto = new AusenciaSaldoDto
            {
                NomeColaborador = collaborator.NomeCompleto,
                SaldoFerias = collaborator.SaldoFerias,
                DiasPendentes = diasPendentes
            };

            return Ok(saldoDto);
        }
    }
}
