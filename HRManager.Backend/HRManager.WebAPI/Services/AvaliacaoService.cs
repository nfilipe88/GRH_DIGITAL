using FluentValidation;
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

        public AvaliacaoService(HRManagerDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
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
            // Busca o ID do Gestor (User)
            var gestor = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailGestor);
            if (gestor == null) throw new KeyNotFoundException("Gestor não encontrado.");

            // Retorna avaliações onde ele é o avaliador
            return await _context.Avaliacoes
                .Include(a => a.Colaborador)
                .Include(a => a.Ciclo)
                .Where(a => a.GestorId == gestor.Id) // Assumindo que Avaliacao tem GestorId (int) ligado a User.Id
                .Select(a => MapToDto(a))
                .ToListAsync();
        }

        public async Task<AvaliacaoDto> IniciarAvaliacaoAsync(int colaboradorId, int cicloId, string emailGestor)
        {
            // Validar existência
            var existe = await _context.Avaliacoes
                .AnyAsync(a => a.ColaboradorId == colaboradorId && a.CicloId == cicloId);
            if (existe) throw new ValidationException("Este colaborador já tem uma avaliação neste ciclo.");

            // Obter Gestor e Colaborador (para saber a Instituição)
            var gestor = await _context.Users.FirstAsync(u => u.Email == emailGestor);
            var colaborador = await _context.Colaboradores.FindAsync(colaboradorId);
            
            if (colaborador == null) throw new KeyNotFoundException("Colaborador não encontrado.");

            var novaAvaliacao = new Avaliacao
            {
                ColaboradorId = colaboradorId,
                CicloId = cicloId,
                GestorId = gestor.Id,
                InstituicaoId = colaborador.InstituicaoId, // <--- GARANTIR SEGURANÇA
                Estado = EstadoAvaliacao.NaoIniciada,
                DataCriacao = DateTime.UtcNow
            };

            // Copiar competências ativas da instituição (se houver filtro) ou gerais
            var competencias = await _context.Competencias.Where(c => c.IsAtiva).ToListAsync();
            
            novaAvaliacao.Itens = competencias.Select(c => new AvaliacaoItem
            {
                CompetenciaId = c.Id,
                // Notas a null inicialmente
            }).ToList();

            _context.Avaliacoes.Add(novaAvaliacao);
            await _context.SaveChangesAsync();

            return MapToDto(novaAvaliacao);
        }

        public async Task<AvaliacaoDto> SubmeterAvaliacaoAsync(int avaliacaoId, SubmeterAvaliacaoRequest request, string emailAvaliador)
        {
            var avaliacao = await _context.Avaliacoes
                .Include(a => a.Itens)
                .Include(a => a.Colaborador)
                .FirstOrDefaultAsync(a => a.Id == avaliacaoId);

            if (avaliacao == null) throw new KeyNotFoundException("Avaliação não encontrada.");

            foreach (var resposta in request.Respostas)
            {
                var item = avaliacao.Itens.FirstOrDefault(i => i.Id == resposta.ItemId);
                if (item != null)
                {
                    item.NotaGestor = resposta.Nota;
                    // CORREÇÃO: Descomentar para salvar o comentário
                    item.JustificativaGestor = resposta.Comentario; 
                }
            }

            if (request.Finalizar)
            {
                // Calcular Média
                var count = avaliacao.Itens.Count(i => i.NotaGestor.HasValue && i.NotaGestor > 0);
                var soma = avaliacao.Itens.Sum(i => i.NotaGestor ?? 0);

                if (count > 0)
                    avaliacao.MediaFinal = (decimal)soma / count;
                else 
                    avaliacao.MediaFinal = 0;

                avaliacao.Estado = EstadoAvaliacao.Finalizada; // Assumindo enum 'Concluida' ou 'Finalizada'
                avaliacao.DataConclusao = DateTime.UtcNow;
                avaliacao.ComentarioFinalGestor = request.ComentarioFinal;

                // Notificar
                await _notificationService.NotifyUserByEmailAsync(
                    avaliacao.Colaborador.EmailPessoal,
                    "Avaliação Concluída",
                    "A sua avaliação de desempenho foi finalizada.",
                    "/minhas-avaliacoes");
            }
            else
            {
                avaliacao.Estado = EstadoAvaliacao.EmAndamento;
            }

            await _context.SaveChangesAsync();
            return MapToDto(avaliacao);
        }

        // --- IMPLEMENTAÇÃO DOS MÉTODOS EM FALTA ---

        public async Task<List<AvaliacaoDto>> GetMinhasAvaliacoesAsync(string emailColaborador)
        {
            var colaborador = await _context.Colaboradores.FirstOrDefaultAsync(c => c.EmailPessoal == emailColaborador);
            if (colaborador == null) return new List<AvaliacaoDto>();

            return await _context.Avaliacoes
                .Include(a => a.Ciclo)
                .Include(a => a.Itens)
                .Where(a => a.ColaboradorId == colaborador.Id && a.Estado == EstadoAvaliacao.Finalizada) // Só vê concluídas?
                .Select(a => MapToDto(a))
                .ToListAsync();
        }

        // Método auxiliar não exposto na interface, mas útil se precisarmos recalcular
        public async Task<decimal> CalcularMediaFinalAsync(int avaliacaoId)
        {
             var avaliacao = await _context.Avaliacoes.Include(a => a.Itens).FirstOrDefaultAsync(a => a.Id == avaliacaoId);
             if (avaliacao == null) return 0;
             
             var count = avaliacao.Itens.Count(i => i.NotaGestor.HasValue);
             if (count == 0) return 0;
             
             return (decimal)avaliacao.Itens.Sum(i => i.NotaGestor ?? 0) / count;
        }

        public Task<CicloAvaliacaoDto> CriarCicloAsync(CriarCicloRequest request)
        {
             // Implementação futura para criar ciclos via API
             throw new NotImplementedException(); 
        }

        // Helper
        private static AvaliacaoDto MapToDto(Avaliacao a)
        {
            return new AvaliacaoDto
            {
                Id = a.Id,
                NomeColaborador = a.Colaborador?.NomeCompleto ?? "N/A",
                NomeCiclo = a.Ciclo?.Nome ?? "N/A", // Verifique se é .Nome ou .Descricao
                Estado = a.Estado,
                NotaFinal = a.MediaFinal,
                Itens = a.Itens?.Select(i => new AvaliacaoItemDto
                {
                    Id = i.Id,
                    CompetenciaId = i.CompetenciaId,
                    NotaGestor = i.NotaGestor,
                    Comentario = i.JustificativaGestor // Adicionar mapeamento do comentário
                }).ToList() ?? new List<AvaliacaoItemDto>()
            };
        }
    }
}
