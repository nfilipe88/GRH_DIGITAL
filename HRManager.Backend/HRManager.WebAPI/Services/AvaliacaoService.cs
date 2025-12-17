using FluentValidation;
using HRManager.Application.Interfaces;
using HRManager.WebAPI.Domain.enums;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Services
{
    public class AvaliacaoService : IAvaliacaoService
    {
        private readonly HRManagerDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ITenantService _tenantService;

        public AvaliacaoService(HRManagerDbContext context, INotificationService notificationService, ITenantService tenantService)
        {
            _context = context;
            _notificationService = notificationService;
            _tenantService=tenantService;
        }

        public async Task<List<CicloAvaliacaoDto>> GetCiclosAsync()
        {
            return await _context.CiclosAvaliacao
                .OrderByDescending(c => c.DataInicio)
                .Select(c => new CicloAvaliacaoDto
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    DataInicio = c.DataInicio,
                    DataFim = c.DataFim,
                    IsAtivo = c.IsAtivo
                })
                .ToListAsync();
        }

        public async Task<List<AvaliacaoDto>> GetAvaliacoesEquipaAsync(string emailGestor)
        {
            var gestor = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailGestor);
            if (gestor == null) throw new KeyNotFoundException("Gestor não encontrado.");

            // 1. Trazer dados da BD (Resolve erro de Expression Tree CS0854)
            var avaliacoes = await _context.Avaliacoes
                .Include(a => a.Colaborador)
                .Include(a => a.Ciclo)
                .Include(a => a.Itens).ThenInclude(i => i.Competencia)
                .Where(a => a.GestorId == gestor.Id)
                .ToListAsync(); // <--- Executa a query aqui

            // 2. Mapear em memória
            return avaliacoes.Select(a => MapToDto(a)).ToList();
        }

        // --- 1. LÓGICA DO COLABORADOR (NOVO) ---
        public async Task<AvaliacaoDto> RealizarAutoAvaliacaoAsync(Guid avaliacaoId, RealizarAutoAvaliacaoRequest request, string emailColaborador)
        {
            var avaliacao = await _context.Avaliacoes
                .Include(a => a.Itens).ThenInclude(i => i.Competencia)
                .Include(a => a.Colaborador)
                .Include(a => a.Ciclo) // Include Ciclo para o MapToDto não falhar
                .FirstOrDefaultAsync(a => a.Id == avaliacaoId);

            if (avaliacao == null) throw new KeyNotFoundException("Avaliação não encontrada.");

            // Segurança: Garantir que é o próprio colaborador
            if (avaliacao.Colaborador.EmailPessoal != emailColaborador)
                throw new UnauthorizedAccessException("Não pode editar a autoavaliação de outro colaborador.");

            // Regra: Só pode editar se não tiver sido finalizada pelo gestor
            if (avaliacao.Estado == EstadoAvaliacao.Finalizada || avaliacao.Estado == EstadoAvaliacao.Cancelada)
                throw new ValidationException("Esta avaliação já está fechada.");

            // Atualizar Itens
            foreach (var resp in request.Respostas)
            {
                var item = avaliacao.Itens.FirstOrDefault(i => i.Id == resp.ItemId);
                if (item != null)
                {
                    item.NotaAutoAvaliacao = resp.Nota;
                    item.JustificativaColaborador = resp.Comentario;
                }
            }

            // Mudar estado
            if (request.Finalizar)
            {
                // Se finalizou a autoavaliação, passa a bola para o gestor
                avaliacao.Estado = EstadoAvaliacao.EmAndamento;
                // Poderíamos ter um estado 'AguardandoGestor', mas 'EmAndamento' serve para dizer que o processo iniciou.
            }

            await _context.SaveChangesAsync();
            return MapToDto(avaliacao);
        }

        // --- 2. LÓGICA DO GESTOR (ATUALIZADO) ---
        // Renomeei de SubmeterAvaliacaoAsync para RealizarAvaliacaoGestorAsync para ser explícito
        public async Task<AvaliacaoDto> RealizarAvaliacaoGestorAsync(Guid avaliacaoId, RealizarAvaliacaoGestorRequest request, string emailGestor)
        {
            var avaliacao = await _context.Avaliacoes
                .Include(a => a.Itens)
                .Include(a => a.Colaborador)
                .FirstOrDefaultAsync(a => a.Id == avaliacaoId);

            if (avaliacao == null) throw new KeyNotFoundException("Avaliação não encontrada.");

            // --- NOVA VALIDAÇÃO DE SEGURANÇA ---
            // 1. Busca o utilizador que está a tentar fazer a ação
            var gestorLogado = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailGestor);
            if (gestorLogado == null) throw new UnauthorizedAccessException("Utilizador não identificado.");

            // 2. Verifica se o Gestor da avaliação é REALMENTE quem está logado
            if (avaliacao.GestorId != gestorLogado.Id)
            {
                // Se fores SuperAdmin talvez possas permitir, mas por defeito bloqueamos:
                throw new UnauthorizedAccessException("Não tem permissão para avaliar este colaborador.");
            }
            // -----------------------------------

            foreach (var resposta in request.Respostas)
            {
                var item = avaliacao.Itens.FirstOrDefault(i => i.Id == resposta.ItemId);
                if (item != null)
                {
                    item.NotaGestor = resposta.Nota;
                    item.JustificativaGestor = resposta.Comentario;
                }
            }

            if (request.Finalizar)
            {
                var count = avaliacao.Itens.Count(i => i.NotaGestor.HasValue && i.NotaGestor > 0);
                var soma = avaliacao.Itens.Sum(i => i.NotaGestor ?? 0);

                avaliacao.MediaFinal = count > 0 ? (decimal)soma / count : 0;
                avaliacao.Estado = EstadoAvaliacao.Finalizada;
                avaliacao.DataConclusao = DateTime.UtcNow;
                avaliacao.ComentarioFinalGestor = request.ComentarioFinal;

                await _notificationService.NotifyUserByEmailAsync(
                    avaliacao.Colaborador.EmailPessoal,
                    "Avaliação Disponível",
                    $"O seu gestor finalizou a sua avaliação. Nota Final: {avaliacao.MediaFinal:F2}",
                    "/minhas-avaliacoes");
            }

            await _context.SaveChangesAsync();
            return MapToDto(avaliacao);
        }
        public async Task<List<AvaliacaoDto>> GetMinhasAvaliacoesAsync(string emailColaborador)
        {
            var colaborador = await _context.Colaboradores.FirstOrDefaultAsync(c => c.EmailPessoal == emailColaborador);
            if (colaborador == null) return new List<AvaliacaoDto>();

            var avaliacoes = await _context.Avaliacoes
                .Include(a => a.Ciclo)
                .Include(a => a.Itens)
                .ThenInclude(i => i.Competencia)
                .Where(a => a.ColaboradorId == colaborador.Id)
                .ToListAsync();

            // AQUI ESTÁ A CORREÇÃO DE SEGURANÇA:
            // Passamos 'true' para indicar que é a visão do colaborador (deve esconder notas)
            return avaliacoes.Select(a => MapToDto(a, isCollaboratorView: true)).ToList();
        }

        // Método auxiliar não exposto na interface, mas útil se precisarmos recalcular
        public async Task<decimal> CalcularMediaFinalAsync(Guid avaliacaoId)
        {
             var avaliacao = await _context.Avaliacoes.Include(a => a.Itens).FirstOrDefaultAsync(a => a.Id == avaliacaoId);
             if (avaliacao == null) return 0;
             
             var count = avaliacao.Itens.Count(i => i.NotaGestor.HasValue);
             if (count == 0) return 0;
             
             return (decimal)avaliacao.Itens.Sum(i => i.NotaGestor ?? 0) / count;
        }

        // ====================================================================
        // FASE 1: CONFIGURAÇÃO (GESTÃO DE COMPETÊNCIAS E CICLOS)
        // ====================================================================

        public async Task<Competencia> CriarCompetenciaAsync(CriarCompetenciaRequest request)
        {
            var tenantId = _tenantService.GetInstituicaoId(); // Garante segurança

            var novaCompetencia = new Competencia
            {
                Nome = request.Nome,
                Descricao = request.Descricao,
                Tipo = request.Tipo,
                IsAtiva = true, // Nasce ativa
                InstituicaoId = tenantId
            };

            _context.Competencias.Add(novaCompetencia);
            await _context.SaveChangesAsync();
            return novaCompetencia;
        }

        public async Task<List<Competencia>> GetCompetenciasAsync()
        {
            // O Global Filter do DbContext deve tratar do TenantId, 
            // mas aqui listamos apenas as ativas ou todas para gestão
            return await _context.Competencias
                .OrderBy(c => c.Nome)
                .ToListAsync();
        }

        public async Task<bool> ToggleCompetenciaStatusAsync(Guid id)
        {
            var comp = await _context.Competencias.FindAsync(id);
            if (comp == null) throw new KeyNotFoundException("Competência não encontrada.");

            comp.IsAtiva = !comp.IsAtiva;
            await _context.SaveChangesAsync();
            return comp.IsAtiva;
        }

        public async Task<CicloAvaliacaoDto> CriarCicloAsync(CriarCicloRequest request)
        {
            var tenantId = _tenantService.GetInstituicaoId();

            // Validar datas
            if (request.DataFim <= request.DataInicio)
                throw new ValidationException("A data de fim deve ser superior à de início.");

            // Desativar ciclos anteriores automaticamente? (Opcional, mas boa prática)
            // var ciclosAtivos = await _context.CiclosAvaliacao.Where(c => c.IsAtivo).ToListAsync();
            // ciclosAtivos.ForEach(c => c.IsAtivo = false);

            var novoCiclo = new CicloAvaliacao
            {
                Nome = request.Nome,
                DataInicio = DateTime.SpecifyKind(request.DataInicio, DateTimeKind.Utc),
                DataFim = DateTime.SpecifyKind(request.DataFim, DateTimeKind.Utc),
                IsAtivo = true,
                InstituicaoId = tenantId
            };

            _context.CiclosAvaliacao.Add(novoCiclo);
            await _context.SaveChangesAsync();

            return new CicloAvaliacaoDto
            {
                Id = novoCiclo.Id,
                Nome = novoCiclo.Nome,
                DataInicio = novoCiclo.DataInicio,
                DataFim = novoCiclo.DataFim,
                IsAtivo = novoCiclo.IsAtivo
            };
        }

        // --- REVISÃO DO MÉTODO CRÍTICO: INICIAR AVALIAÇÃO ---
        // Este é o momento onde as "Questões" são lançadas para o colaborador
        public async Task<AvaliacaoDto> IniciarAvaliacaoAsync(Guid colaboradorId, Guid cicloId, string emailGestor)
        {
            // 1. Validações Básicas
            var existe = await _context.Avaliacoes
                .AnyAsync(a => a.ColaboradorId == colaboradorId && a.CicloId == cicloId);
            if (existe) throw new ValidationException("Avaliação já iniciada para este colaborador neste ciclo.");

            var gestor = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailGestor);
            var colaborador = await _context.Colaboradores.FindAsync(colaboradorId);

            if (gestor == null || colaborador == null)
                throw new KeyNotFoundException("Gestor ou Colaborador não encontrados.");

            // 2. Buscar as "PERGUNTAS" (Competências Ativas da Instituição)
            // Importante: O Global Query Filter no DbContext garante que só pegamos da Instituição correta.
            var competenciasAtivas = await _context.Competencias
                .Where(c => c.IsAtiva)
                .ToListAsync();

            if (!competenciasAtivas.Any())
                throw new ValidationException("Não existem competências ativas configuradas. O RH deve configurar as questões primeiro.");

            // 3. Criar a Avaliação e Copiar as Perguntas para AvaliacaoItem
            var novaAvaliacao = new Avaliacao
            {
                ColaboradorId = colaboradorId,
                CicloId = cicloId,
                GestorId = gestor.Id,
                InstituicaoId = colaborador.InstituicaoId,
                Estado = EstadoAvaliacao.NaoIniciada,
                DataCriacao = DateTime.UtcNow,

                // AQUI ESTÁ O "LANÇAMENTO" DAS QUESTÕES:
                Itens = competenciasAtivas.Select(c => new AvaliacaoItem
                {
                    CompetenciaId = c.Id,
                    NotaAutoAvaliacao = null,
                    NotaGestor = null
                }).ToList()
            };

            _context.Avaliacoes.Add(novaAvaliacao);
            await _context.SaveChangesAsync();

            // 4. Notificar
            await _notificationService.NotifyUserByEmailAsync(
                colaborador.EmailPessoal,
                "Avaliação Iniciada",
                $"O ciclo '{novaAvaliacao.Ciclo?.Nome}' começou. Aceda para realizar sua autoavaliação.",
                "/minhas-avaliacoes"
            );

            // Precisamos recarregar para trazer os Includes no MapToDto
            return await GetAvaliacaoPorIdInternalAsync(novaAvaliacao.Id);
        }

        // Método auxiliar privado para buscar ID com includes (evita repetição)
        private async Task<AvaliacaoDto> GetAvaliacaoPorIdInternalAsync(Guid id)
        {
            var avaliacao = await _context.Avaliacoes
               .Include(a => a.Colaborador)
               .Include(a => a.Ciclo)
               .Include(a => a.Itens)
                   .ThenInclude(i => i.Competencia) // <--- CRÍTICO: Incluir a Competência para saber o Nome da Pergunta
               .FirstOrDefaultAsync(a => a.Id == id);

            if (avaliacao == null) return null;
            return MapToDto(avaliacao);
        }

        // Helper MapToDto Atualizado
        private static AvaliacaoDto MapToDto(Avaliacao a, bool isCollaboratorView = false)
        {
            bool esconderNotasGestor = isCollaboratorView && a.Estado != EstadoAvaliacao.Finalizada;

            return new AvaliacaoDto
            {
                Id = a.Id,
                NomeColaborador = a.Colaborador?.NomeCompleto ?? "N/A",
                NomeGestor = a.Gestor?.NomeCompleto ?? "N/A", // Assumindo que carregamos o gestor se necessário
                NomeCiclo = a.Ciclo?.Nome ?? "N/A",
                Estado = a.Estado,
                NotaFinal = esconderNotasGestor ? 0 : a.MediaFinal,
                DataConclusao = a.DataConclusao,
                ComentarioFinalGestor = esconderNotasGestor ? null : a.ComentarioFinalGestor,
                Itens = a.Itens?.Select(i => new AvaliacaoItemDto
                {
                    Id = i.Id,
                    CompetenciaId = i.CompetenciaId,
                    NomeCompetencia = i.Competencia?.Nome ?? "N/A",
                    NotaAutoAvaliacao = i.NotaAutoAvaliacao,
                    JustificativaColaborador = i.JustificativaColaborador,
                    NotaGestor = esconderNotasGestor ? null : i.NotaGestor,
                    Comentario = esconderNotasGestor ? null : i.JustificativaGestor
                }).ToList() ?? new List<AvaliacaoItemDto>()
            };
        }
    }
}
