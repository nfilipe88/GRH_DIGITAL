using HRManager.Application.Interfaces;
using HRManager.WebAPI.Domain.enums;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HRManager.WebAPI.Services
{
    public class AusenciaService : IAusenciaService
    {
        private readonly HRManagerDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public AusenciaService(
            HRManagerDbContext context,
            ITenantService tenantService,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _context = context;
            _tenantService = tenantService;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<List<AusenciaDto>> GetAusenciasAsync(string userEmail, bool isGestorRH, bool isGestorMaster)
        {
            var query = _context.Ausencias.AsQueryable();

            // 1. Filtros de Segurança
            if (isGestorRH)
            {
                // O Global Query Filter já filtra por Tenant, mas garantimos a lógica aqui
                var tenantId = _tenantService.GetTenantId();
                if (tenantId.HasValue)
                {
                    query = query.Where(a => a.Colaborador.InstituicaoId == tenantId.Value);
                }
            }
            else if (!isGestorMaster) // É Colaborador
            {
                query = query.Where(a => a.Colaborador.EmailPessoal == userEmail);
            }

            // 2. Projeção
            return await query
                .Include(a => a.Colaborador)
                .Select(a => new AusenciaDto
                {
                    Id = a.Id,
                    NomeColaborador = a.Colaborador.NomeCompleto,
                    Tipo = a.Tipo.ToString(),
                    DataInicio = a.DataInicio,
                    DataFim = a.DataFim,
                    DiasTotal = (a.DataFim - a.DataInicio).Days + 1,
                    Estado = a.Estado.ToString(),
                    DataSolicitacao = a.DataSolicitacao,
                    CaminhoDocumento = a.CaminhoDocumento,
                })
                .OrderByDescending(a => a.DataSolicitacao)
                .ToListAsync();
        }

        public async Task<AusenciaSaldoDto> GetSaldoAsync(string userEmail)
        {
            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.EmailPessoal == userEmail);

            if (colaborador == null)
                throw new KeyNotFoundException("Colaborador não encontrado.");

            // Calcular dias "em análise" (apenas para Férias)
            var diasPendentes = await _context.Ausencias
                .Where(a => a.ColaboradorId == colaborador.Id
                            && a.Tipo == TipoAusencia.Ferias
                            && a.Estado == EstadoAusencia.Pendente)
                .SumAsync(a => (a.DataFim - a.DataInicio).Days + 1);

            return new AusenciaSaldoDto
            {
                NomeColaborador = colaborador.NomeCompleto,
                SaldoFerias = colaborador.SaldoFerias,
                DiasPendentes = diasPendentes
            };
        }

        public async Task SolicitarAusenciaAsync(string userEmail, CriarAusenciaRequest request)
        {
            // 1. Recuperar Colaborador
            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.EmailPessoal == userEmail);

            if (colaborador == null)
                throw new KeyNotFoundException("Perfil de colaborador não encontrado.");

            // 2. Normalizar Datas (UTC)
            var dataInicioUtc = DateTime.SpecifyKind(request.DataInicio, DateTimeKind.Utc);
            var dataFimUtc = DateTime.SpecifyKind(request.DataFim, DateTimeKind.Utc);

            // 3. Validação de Negócio: Conflito de Datas
            bool existeConflito = await _context.Ausencias
                .AnyAsync(a => a.ColaboradorId == colaborador.Id
                                && a.Estado != EstadoAusencia.Rejeitada
                                && a.Estado != EstadoAusencia.Cancelada
                                && a.DataInicio <= dataFimUtc
                                && a.DataFim >= dataInicioUtc);

            if (existeConflito)
                throw new ValidationException("Já existe uma solicitação registada para este período.");

            // 4. Validação de Negócio: Saldo de Férias
            if (request.Tipo == TipoAusencia.Ferias)
            {
                int diasSolicitados = (request.DataFim - request.DataInicio).Days + 1;

                int diasJaPendentes = await _context.Ausencias
                    .Where(a => a.ColaboradorId == colaborador.Id
                                && a.Tipo == TipoAusencia.Ferias
                                && a.Estado == EstadoAusencia.Pendente)
                    .SumAsync(a => (a.DataFim - a.DataInicio).Days + 1);

                if (colaborador.SaldoFerias < (diasSolicitados + diasJaPendentes))
                {
                    throw new ValidationException($"Saldo insuficiente. Disponível: {colaborador.SaldoFerias}. Cativos: {diasJaPendentes}.");
                }
            }

            // 5. Gestão de Ficheiro (Upload)
            string? caminhoFicheiro = null;
            if (request.Documento != null && request.Documento.Length > 0)
            {
                // Nota: Em arquitetura limpa, isto poderia ser um IStorageService, mas aqui funciona bem.
                var pastaUploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(pastaUploads)) Directory.CreateDirectory(pastaUploads);

                var nomeFicheiro = Guid.NewGuid().ToString() + Path.GetExtension(request.Documento.FileName);
                var caminhoCompleto = Path.Combine(pastaUploads, nomeFicheiro);

                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await request.Documento.CopyToAsync(stream);
                }
                caminhoFicheiro = "uploads/" + nomeFicheiro;
            }

            // 6. Persistência
            var novaAusencia = new Ausencia
            {
                ColaboradorId = colaborador.Id,
                Tipo = request.Tipo,
                DataInicio = dataInicioUtc,
                DataFim = dataFimUtc,
                Motivo = request.Motivo,
                CaminhoDocumento = caminhoFicheiro,
                Estado = EstadoAusencia.Pendente,
                DataSolicitacao = DateTime.UtcNow
            };

            _context.Ausencias.Add(novaAusencia);
            await _context.SaveChangesAsync();

            // 7. Notificação
            await _notificationService.NotifyManagersAsync(
                colaborador.InstituicaoId,
                "Novo Pedido de Ausência",
                $"{colaborador.NomeCompleto} solicitou {request.Tipo} ({request.DataInicio:dd/MM}).",
                "/gestao-ausencias"
            );
        }

        public async Task ResponderAusenciaAsync(int id, ResponderAusenciaRequest request, string userEmail, bool isGestorRH)
        {
            var ausencia = await _context.Ausencias
                .Include(a => a.Colaborador)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (ausencia == null)
                throw new KeyNotFoundException("Pedido não encontrado.");

            // Validação Multi-Tenant manual (Safety Check)
            // Se o Global Filter estiver ativo, 'ausencia' já será null se for de outra Tenant,
            // mas validamos explicitamente para garantir integridade.
            if (isGestorRH)
            {
                var tenantId = _tenantService.GetTenantId();
                if (tenantId.HasValue && ausencia.Colaborador.InstituicaoId != tenantId.Value)
                    throw new UnauthorizedAccessException("Não tem permissão para gerir esta ausência.");
            }

            if (ausencia.Estado != EstadoAusencia.Pendente)
                throw new ValidationException($"Este pedido já foi processado ({ausencia.Estado}).");

            // Lógica de Aprovação/Rejeição
            if (request.Aprovado)
            {
                if (ausencia.Tipo == TipoAusencia.Ferias)
                {
                    int dias = (ausencia.DataFim - ausencia.DataInicio).Days + 1;
                    if (ausencia.Colaborador.SaldoFerias < dias)
                        throw new ValidationException("Não é possível aprovar: Saldo insuficiente.");

                    // Atualizar Saldo
                    ausencia.Colaborador.SaldoFerias -= dias;
                }
                ausencia.Estado = EstadoAusencia.Aprovada;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.Comentario))
                    throw new ValidationException("Justificativa obrigatória para rejeição.");

                ausencia.Estado = EstadoAusencia.Rejeitada;
            }

            ausencia.ComentarioGestor = request.Comentario;
            ausencia.DataResposta = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Notificações e Email
            await EnviarNotificacoesResposta(ausencia, request.Aprovado, request.Comentario);
        }

        public Task<byte[]> DownloadRelatorioExcelAsync(int mes, int ano)
        {
            // Implementação futura ou mover lógica do RelatoriosController para aqui
            throw new NotImplementedException();
        }

        // --- Helpers Privados ---

        private async Task EnviarNotificacoesResposta(Ausencia ausencia, bool aprovado, string? comentario)
        {
            string resultado = aprovado ? "Aprovado ✅" : "Rejeitado ❌";

            // 1. Notificação In-App
            await _notificationService.NotifyUserByEmailAsync(
                ausencia.Colaborador.EmailPessoal,
                $"Pedido {resultado}",
                $"O seu pedido foi {resultado.ToLower()}.",
                "/minhas-ausencias"
            );

            // 2. Email (Fire and Forget seguro)
            try
            {
                string cor = aprovado ? "green" : "red";
                var sb = new StringBuilder();
                sb.AppendLine($"<h2>Olá, {ausencia.Colaborador.NomeCompleto}</h2>");
                sb.AppendLine($"<p>O seu pedido de ausência para <strong>{ausencia.DataInicio:dd/MM}</strong> foi processado.</p>");
                sb.AppendLine($"<p>Estado: <strong style='color:{cor}'>{resultado}</strong></p>");

                if (!string.IsNullOrEmpty(comentario))
                    sb.AppendLine($"<p>Comentário: <em>{comentario}</em></p>");

                await _emailService.SendEmailAsync(
                    ausencia.Colaborador.EmailPessoal,
                    $"HR-Manager: Pedido {resultado}",
                    sb.ToString()
                );
            }
            catch (Exception ex)
            {
                // Log de erro de email (console por enquanto)
                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
            }
        }

        private async Task EnviarNotificacaoConclusao(Ausencia ausencia, bool aprovado, string? comentario)
        {
            string resultado = aprovado ? "Aprovado ✅" : "Rejeitado ❌";
            string cor = aprovado ? "green" : "red";

            // 1. Notificação In-App
            await _notificationService.NotifyUserByEmailAsync(
                ausencia.Colaborador.EmailPessoal,
                $"Pedido {resultado}",
                $"O seu pedido de {ausencia.Tipo} foi {resultado.ToLower()}.",
                "/minhas-ausencias"
            );

            // 2. Email
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"<h2>Olá, {ausencia.Colaborador.NomeCompleto}</h2>");
                sb.AppendLine($"<p>O seu pedido para o período <strong>{ausencia.DataInicio:dd/MM}</strong> a <strong>{ausencia.DataFim:dd/MM}</strong> foi processado.</p>");
                sb.AppendLine($"<p>Estado: <strong style='color:{cor}'>{resultado}</strong></p>");

                if (!string.IsNullOrEmpty(comentario))
                    sb.AppendLine($"<p>Obs: <em>{comentario}</em></p>");

                await _emailService.SendEmailAsync(
                    ausencia.Colaborador.EmailPessoal,
                    $"HR-Manager: Pedido {resultado}",
                    sb.ToString()
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar email (ignorado para não bloquear): {ex.Message}");
            }
        }
    }
}
