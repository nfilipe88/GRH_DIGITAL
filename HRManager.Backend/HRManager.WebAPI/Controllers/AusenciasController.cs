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
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requer login para tudo
    public class AusenciasController : ControllerBase
    {
        private readonly HRManagerDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public AusenciasController(HRManagerDbContext context, ITenantService tenantService, IEmailService emailService, INotificationService notificationService)
        {
            _context = context;
            _tenantService = tenantService;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        // ---
        // GET: Listar Ausências (Inteligente)
        // ---
        [HttpGet]
        public async Task<IActionResult> GetAusencias()
        {
            var query = _context.Ausencias.AsQueryable();

            // 1. Se for GestorRH, vê apenas da sua instituição
            if (User.IsInRole("GestorRH"))
            {
                var tenantId = _tenantService.GetTenantId();
                if (tenantId.HasValue)
                {
                    query = query.Where(a => a.Colaborador.InstituicaoId == tenantId.Value);
                }
            }
            // 2. Se for GestorMaster, vê tudo (sem filtro)

            // 3. Se não for nenhum dos anteriores, assume que é um Colaborador e vê SÓ as suas
            // (Nota: No futuro, teremos o Role "Colaborador". Por agora, se não é gestor, filtramos pelo email)
            else if (!User.IsInRole("GestorMaster"))
            {
                var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
                query = query.Where(a => a.Colaborador.EmailPessoal == email);
            }

            var lista = await query
                .Include(a => a.Colaborador) // Necessário para aceder ao Nome
                .Include(a => a.Colaborador.Instituicao) // Necessário para aceder ao Nome da Instituição (se preciso)
                .Select(a => new AusenciaDto
                {
                    Id = a.Id,
                    NomeColaborador = a.Colaborador.NomeCompleto,
                    Tipo = a.Tipo.ToString(),
                    DataInicio = a.DataInicio,
                    DataFim = a.DataFim,
                    // Cálculo simplificado de dias (inclui fins de semana por agora)
                    DiasTotal = (a.DataFim - a.DataInicio).Days + 1,
                    Estado = a.Estado.ToString(),
                    DataSolicitacao = a.DataSolicitacao,
                    CaminhoDocumento = a.CaminhoDocumento,
                })
                .OrderByDescending(a => a.DataSolicitacao)
                .ToListAsync();

            return Ok(lista);
        }

        // ---
        // GET: Obter Saldo (Para o Dashboard do Colaborador)
        // ---
        [HttpGet("saldo")]
        public async Task<IActionResult> GetMeuSaldo()
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            if (string.IsNullOrEmpty(email)) return Unauthorized(new { message = "Email não encontrado no token." });

            var collaborator = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.EmailPessoal == email);

            if (collaborator == null) return NotFound(new { message = "Colaborador não encontrado." });

            // Calcular dias "em análise" (apenas para Férias)
            // Nota: A função EF.Functions.DateDiffDay poderia ser usada, mas a subtração direta funciona no EF Core moderno
            var diasPendentes = await _context.Ausencias
                .Where(a => a.ColaboradorId == collaborator.Id
                            && a.Tipo == TipoAusencia.Ferias
                            && a.Estado == EstadoAusencia.Pendente)
                .SumAsync(a => (a.DataFim - a.DataInicio).Days + 1);

            var saldoDto = new AusenciaSaldoDto
            {
                NomeColaborador = collaborator.NomeCompleto,
                SaldoFerias = collaborator.SaldoFerias,
                DiasPendentes = diasPendentes
            };

            return Ok(saldoDto);
        }

        // ---
        // POST: Solicitar Ausência
        // ---
        [HttpPost]
        public async Task<IActionResult> SolicitarAusencia([FromBody] CriarAusenciaRequest request)
        {
            // 1. Identificar o colaborador
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            var colaborador = await _context.Colaboradores.FirstOrDefaultAsync(c => c.EmailPessoal == email);

            if (colaborador == null) return BadRequest(new { message = "Perfil de colaborador não encontrado para este utilizador." });

            // 2. Validar Datas Básicas
            if (request.DataInicio > request.DataFim)
            {
                return BadRequest(new { message = "A data de fim deve ser igual ou posterior à data de início." });
            }
            // *** Converter para UTC ANTES da query ***
            var dataInicioUtc = DateTime.SpecifyKind(request.DataInicio, DateTimeKind.Utc);
            var dataFimUtc = DateTime.SpecifyKind(request.DataFim, DateTimeKind.Utc);

            if (dataInicioUtc.Date < DateTime.UtcNow.Date)
            {
                return BadRequest(new { message = "Não pode solicitar ausências no passado." });
            }

            // --- VALIDAÇÃO 1: CONFLITO DE DATAS ---
            // Verifica se existe alguma ausência (que não esteja Rejeitada ou Cancelada)
            // cujo período se sobreponha ao novo pedido.
            // Agora usamos as variáveis locais (dataInicioUtc, dataFimUtc) que já são seguras
            bool existeConflito = await _context.Ausencias
                .AnyAsync(a => a.ColaboradorId == colaborador.Id
                                && a.Estado != EstadoAusencia.Rejeitada
                                && a.Estado != EstadoAusencia.Cancelada
                                && a.DataInicio <= dataFimUtc
                                && a.DataFim >= dataInicioUtc);

            if (existeConflito)
            {
                return BadRequest(new { message = "Já existe uma solicitação registada para este período (ou parte dele)." });
            }

            // --- VALIDAÇÃO 2: VERIFICAR SALDO (Apenas para Férias) ---
            if (request.Tipo == TipoAusencia.Ferias)
            {
                int diasSolicitados = (request.DataFim - request.DataInicio).Days + 1;

                // Contar dias que já estão pendentes para não deixar exceder o saldo "virtual"
                int diasJaPendentes = await _context.Ausencias
                    .Where(a => a.ColaboradorId == colaborador.Id
                                && a.Tipo == TipoAusencia.Ferias
                                && a.Estado == EstadoAusencia.Pendente)
                    .SumAsync(a => (a.DataFim - a.DataInicio).Days + 1);

                if (colaborador.SaldoFerias < (diasSolicitados + diasJaPendentes))
                {
                    return BadRequest(new { message = $"Saldo insuficiente. Tem {colaborador.SaldoFerias} dias disponíveis (com {diasJaPendentes} dias já cativos em pedidos pendentes)." });
                }
            }

            string? caminhoFicheiro = null;

            // --- LÓGICA DE UPLOAD ---
            if (request.Documento != null && request.Documento.Length > 0)
            {
                // 1. Definir pasta de destino (dentro de wwwroot/uploads)
                var pastaUploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(pastaUploads)) Directory.CreateDirectory(pastaUploads);

                // 2. Gerar nome único para evitar conflitos (Guid + Extensão original)
                var nomeFicheiro = Guid.NewGuid().ToString() + Path.GetExtension(request.Documento.FileName);
                var caminhoCompleto = Path.Combine(pastaUploads, nomeFicheiro);

                // 3. Guardar o ficheiro
                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await request.Documento.CopyToAsync(stream);
                }

                // 4. Guardar o caminho relativo para a BD
                caminhoFicheiro = "uploads/" + nomeFicheiro;
            }

            // 3. Criar o pedido
            var novaAusencia = new Ausencia
            {
                ColaboradorId = colaborador.Id,
                Tipo = request.Tipo,
                // Assegurar UTC para o Postgres
                DataInicio = dataInicioUtc,
                DataFim = dataFimUtc,
                Motivo = request.Motivo,
                // *** ADICIONAR CAMINHO DOS FICHEIROS ***
                CaminhoDocumento = caminhoFicheiro,
                Estado = EstadoAusencia.Pendente,
                DataSolicitacao = DateTime.UtcNow
            };

            _context.Ausencias.Add(novaAusencia);
            await _context.SaveChangesAsync();

            // *** DISPARAR NOTIFICAÇÃO PARA OS GESTORES ***
            await _notificationService.NotifyManagersAsync(
                colaborador.InstituicaoId,
                "Novo Pedido de Ausência",
                $"{colaborador.NomeCompleto} solicitou {request.Tipo} de {request.DataInicio:dd/MM} a {request.DataFim:dd/MM}.",
                "/gestao-ausencias" // Link para o gestor clicar
            );

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

            // Segurança Multi-Tenant: GestorRH só mexe na sua instituição
            if (User.IsInRole("GestorRH"))
            {
                var tenantId = _tenantService.GetTenantId();
                if (ausencia.Colaborador.InstituicaoId != tenantId)
                {
                    return Forbid(); // 403 Forbidden
                }
            }

            // Evitar alterar pedidos já fechados
            if (ausencia.Estado != EstadoAusencia.Pendente)
            {
                return BadRequest(new { message = $"Este pedido já foi processado ({ausencia.Estado})." });
            }

            // --- LÓGICA DE APROVAÇÃO E DESCONTO DE SALDO ---
            if (request.Aprovado)
            {
                // Se for Férias, descontamos o saldo AGORA
                if (ausencia.Tipo == TipoAusencia.Ferias)
                {
                    int dias = (ausencia.DataFim - ausencia.DataInicio).Days + 1;

                    // Re-validar saldo (pode ter mudado entre o pedido e a aprovação)
                    if (ausencia.Colaborador.SaldoFerias < dias)
                    {
                        return BadRequest(new { message = "Não é possível aprovar: O colaborador já não tem saldo suficiente." });
                    }

                    // AQUI ACONTECE O DESCONTO REAL
                    ausencia.Colaborador.SaldoFerias -= dias;
                }

                ausencia.Estado = EstadoAusencia.Aprovada;
            }
            else // Rejeitar
            {
                if (string.IsNullOrWhiteSpace(request.Comentario))
                {
                    return BadRequest(new { message = "É obrigatório fornecer uma justificativa ao rejeitar." });
                }

                ausencia.Estado = EstadoAusencia.Rejeitada;
            }

            ausencia.ComentarioGestor = request.Comentario;
            ausencia.DataResposta = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // *** DISPARAR NOTIFICAÇÃO PARA O COLABORADOR ***
            string resultado = request.Aprovado ? "Aprovado ✅" : "Rejeitado ❌";
            await _notificationService.NotifyUserByEmailAsync(
                ausencia.Colaborador.EmailPessoal,
                $"Pedido {resultado}",
                $"O seu pedido de ausência foi {resultado.ToLower()}.",
                "/minhas-ausencias"
            );

            // *** 3. ENVIAR NOTIFICAÇÃO POR EMAIL ***
            try
            {
                string assunto = request.Aprovado
                    ? "HR-Manager: Pedido de Ausência Aprovado ✅"
                    : "HR-Manager: Pedido de Ausência Rejeitado ❌";

                string estadoTexto = request.Aprovado ? "Aprovado" : "Rejeitado";
                string cor = request.Aprovado ? "green" : "red";

                var sb = new StringBuilder();
                sb.AppendLine($"<h2>Olá, {ausencia.Colaborador.NomeCompleto}</h2>");
                sb.AppendLine($"<p>O seu pedido de ausência ({ausencia.Tipo}) para o período de <strong>{ausencia.DataInicio:dd/MM/yyyy}</strong> a <strong>{ausencia.DataFim:dd/MM/yyyy}</strong> foi processado.</p>");
                sb.AppendLine($"<p>Estado: <strong style='color:{cor}'>{estadoTexto}</strong></p>");

                if (!string.IsNullOrEmpty(request.Comentario))
                {
                    sb.AppendLine($"<p>Comentário do Gestor: <em>{request.Comentario}</em></p>");
                }

                sb.AppendLine("<br><p>Atenciosamente,<br>A Equipa de RH</p>");

                // Envia o email sem bloquear a resposta da API (fire and forget)
                // Nota: Em produção, idealmente usaria uma Queue, mas para MVP isto serve.
                await _emailService.SendEmailAsync(ausencia.Colaborador.EmailPessoal, assunto, sb.ToString());
            }
            catch (Exception ex)
            {
                // Não queremos que o erro de email falhe a resposta da API,
                // mas convém registar o erro.
                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
            }
            // *** FIM DA NOTIFICAÇÃO ***

            string acao = request.Aprovado ? "aprovado" : "rejeitado";
            return Ok(new { message = $"Pedido {acao} com sucesso." });
        }
    }
}
