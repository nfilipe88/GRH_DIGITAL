using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
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
                var tenantId = _tenantService.GetTenantId();
                if (tenantId.HasValue)
                {
                    var tId = tenantId.Value;
                    // Guard against possible null Colaborador
                    query = query.Where(a => a.Colaborador != null && a.Colaborador.InstituicaoId == tId);
                }
            }
            else if (!isGestorMaster) // É Colaborador
            {
                // Guard against possible null Colaborador
                query = query.Where(a => a.Colaborador != null && a.Colaborador.EmailPessoal == userEmail);
            }

            // 2. Projeção
            return await query
                .Include(a => a.Colaborador)
                .Select(a => new AusenciaDto
                {
                    Id = a.Id,
                    NomeColaborador = a.Colaborador != null ? a.Colaborador.NomeCompleto : string.Empty,
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

        public async Task<AusenciaSaldoDto> GetSaldoAsync(string userId)
        {
            // 1. CORREÇÃO: Validar antes de usar
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Utilizador não identificado.");

            if (!Guid.TryParse(userId, out var userGuid))
                throw new ArgumentException("ID de utilizador inválido.");

            // 2. CORREÇÃO: Usar a variável já convertida na Query (evita erros de tradução do LINQ)
            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.UserId == userGuid);

            if (colaborador == null)
            {
                // Retorna zerado para não quebrar o Frontend se o colaborador ainda não tiver perfil criado
                return new AusenciaSaldoDto
                {
                    NomeColaborador = "Utilizador sem Perfil",
                    SaldoFerias = 0,
                    DiasPendentes = 0
                };
            }

            // Calcular dias "em análise"
            var diasPendentes = await _context.Ausencias
                .Where(a => a.ColaboradorId == colaborador.Id
                            && a.Tipo == TipoAusencia.Ferias
                            && a.Estado == EstadoAusencia.Pendente)
                .SumAsync(a => (a.DataFim - a.DataInicio).Days + 1);

            // Nota: O SaldoFerias já vem calculado na entidade Colaborador (assumindo que tens lá a lógica)
            // Se precisares de calcular "Férias Usadas" dinamicamente, podes descomentar a lógica antiga.

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

        public async Task ResponderAusenciaAsync(Guid id, ResponderAusenciaRequest request, string userEmail, bool isGestorRH)
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
                if (tenantId.HasValue && (ausencia.Colaborador == null || ausencia.Colaborador.InstituicaoId != tenantId.Value))
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
                    if (ausencia.Colaborador == null || ausencia.Colaborador.SaldoFerias < dias)
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

        // --- IMPLEMENTAÇÃO DO EXCEL ---
        public async Task<byte[]> DownloadRelatorioExcelAsync(int mes, int ano)
        {
            // 1. Construir a Query
            var query = _context.Ausencias
                .Include(a => a.Colaborador)
                .AsQueryable();

            // 2. Filtro de Data
            query = query.Where(a => a.DataInicio.Month == mes && a.DataInicio.Year == ano);

            // 3. Filtro de Multi-tenancy (Segurança)
            // O TenantService já lê o ID do utilizador logado através do Token
            var tenantId = _tenantService.GetTenantId();
            if (tenantId.HasValue)
            {
                query = query.Where(a => a.Colaborador != null && a.Colaborador.InstituicaoId == tenantId.Value);
            }

            // 4. Buscar dados
            var dados = await query
                .OrderBy(a => a.DataInicio)
                .ToListAsync();

            // 5. Gerar o Excel em memória
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Ausências");

                // Cabeçalhos
                worksheet.Cell(1, 1).Value = "Colaborador";
                worksheet.Cell(1, 2).Value = "NIF";
                worksheet.Cell(1, 3).Value = "Tipo";
                worksheet.Cell(1, 4).Value = "Início";
                worksheet.Cell(1, 5).Value = "Fim";
                worksheet.Cell(1, 6).Value = "Duração (Dias)";
                worksheet.Cell(1, 7).Value = "Estado";
                worksheet.Cell(1, 8).Value = "Motivo";

                // Estilo
                var headerRange = worksheet.Range("A1:H1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                int row = 2;
                foreach (var item in dados)
                {
                    // Protege contra possível null em Colaborador
                    var nomeColaborador = item.Colaborador?.NomeCompleto ?? string.Empty;
                    var nifColaborador = item.Colaborador?.NIF ?? string.Empty;

                    worksheet.Cell(row, 1).Value = nomeColaborador;
                    worksheet.Cell(row, 2).Value = "'" + nifColaborador; // Aspa para forçar texto
                    worksheet.Cell(row, 3).Value = item.Tipo.ToString();
                    worksheet.Cell(row, 4).Value = item.DataInicio;
                    worksheet.Cell(row, 5).Value = item.DataFim;
                    worksheet.Cell(row, 6).Value = (item.DataFim - item.DataInicio).Days + 1;
                    worksheet.Cell(row, 7).Value = item.Estado.ToString();
                    worksheet.Cell(row, 8).Value = item.Motivo;

                    // Formatação condicional simples
                    if (item.Estado == EstadoAusencia.Aprovada)
                        worksheet.Cell(row, 7).Style.Font.FontColor = XLColor.Green;
                    else if (item.Estado == EstadoAusencia.Rejeitada)
                        worksheet.Cell(row, 7).Style.Font.FontColor = XLColor.Red;
                    else
                        worksheet.Cell(row, 7).Style.Font.FontColor = XLColor.Orange;

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        // --- Helpers Privados ---

        private async Task EnviarNotificacoesResposta(Ausencia ausencia, bool aprovado, string? comentario)
        {
            string resultado = aprovado ? "Aprovado ✅" : "Rejeitado ❌";

            // 1. Notificação In-App
            var email = ausencia.Colaborador?.EmailPessoal;
            if (!string.IsNullOrWhiteSpace(email))
            {
                await _notificationService.NotifyUserByEmailAsync(
                    email,
                    $"Pedido {resultado}",
                    $"O seu pedido foi {resultado.ToLower()}.",
                    "/minhas-ausencias"
                );
            }

            // 2. Email (Fire and Forget seguro)
            try
            {
                if (ausencia.Colaborador != null && !string.IsNullOrWhiteSpace(ausencia.Colaborador.EmailPessoal))
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

            // Guard: evita null dereference se Colaborador ou EmailPessoal forem nulos/empty
            var destinatario = ausencia?.Colaborador?.EmailPessoal;
            if (string.IsNullOrWhiteSpace(destinatario))
            {
                Console.WriteLine($"Não foi possível enviar notificação: email do colaborador em falta. AusenciaId={(ausencia?.Id.ToString() ?? "N/A")}");
                return;
            }

            // 1. Notificação In-App
            await _notificationService.NotifyUserByEmailAsync(
                destinatario,
                $"Pedido {resultado}",
                $"O seu pedido de {(ausencia?.Tipo.ToString() ?? "ausência")} foi {resultado.ToLower()}.",
                "/minhas-ausencias"
            );

            // 2. Email
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"<h2>Olá, {ausencia?.Colaborador?.NomeCompleto ?? "Colaborador"}</h2>");
                sb.AppendLine($"<p>O seu pedido para o período <strong>{ausencia?.DataInicio:dd/MM}</strong> a <strong>{ausencia?.DataFim:dd/MM}</strong> foi processado.</p>");
                sb.AppendLine($"<p>Estado: <strong style='color:{cor}'>{resultado}</strong></p>");

                if (!string.IsNullOrEmpty(comentario))
                    sb.AppendLine($"<p>Obs: <em>{comentario}</em></p>");

                await _emailService.SendEmailAsync(
                    destinatario,
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
