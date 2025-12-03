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
            // 1. Validar se já existe
            var existe = await _context.Avaliacoes
                .AnyAsync(a => a.ColaboradorId == colaboradorId && a.CicloId == cicloId);

            if (existe) throw new ValidationException("Este colaborador já tem uma avaliação neste ciclo.");

            var gestor = await _context.Users.FirstAsync(u => u.Email == emailGestor);

            // 2. Criar a "Capa" da Avaliação
            var novaAvaliacao = new Avaliacao
            {
                ColaboradorId = colaboradorId,
                CicloId = cicloId,
                GestorId = gestor.Id,
                Estado = EstadoAvaliacao.NaoIniciada,
                DataCriacao = DateTime.UtcNow
            };

            // 3. Copiar as Competências do Modelo para Itens de Avaliação
            // (Assumindo que temos uma tabela Competencias com as perguntas padrão)
            var competencias = await _context.Competencias.Where(c => c.IsAtiva).ToListAsync();
            novaAvaliacao.Itens = competencias.Select(c => new AvaliacaoItem
            {
                CompetenciaId = c.Id,
                // Notas começam a null
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

            // TODO: Validar se quem está a submeter é o Gestor ou o Colaborador (Autoavaliação)
            // Para simplificar, assumimos fluxo do Gestor aqui:

            foreach (var resposta in request.Respostas)
            {
                var item = avaliacao.Itens.FirstOrDefault(i => i.Id == resposta.ItemId);
                if (item != null)
                {
                    item.NotaGestor = resposta.Nota;
                    //item.ComentarioGestor = resposta.Comentario;
                }
            }

            if (request.Finalizar)
            {
                // Calcular Média
                var soma = avaliacao.Itens.Sum(i => i.NotaGestor ?? 0);
                var count = avaliacao.Itens.Count(i => i.NotaGestor.HasValue);

                if (count > 0)
                    avaliacao.MediaFinal = (decimal)soma / count;

                avaliacao.Estado = EstadoAvaliacao.Finalizada; // ou AguardandoFeedback
                avaliacao.DataConclusao = DateTime.UtcNow;
                avaliacao.ComentarioFinalGestor = request.ComentarioFinal;

                // Notificar Colaborador
                await _notificationService.NotifyUserByEmailAsync(
                    avaliacao.Colaborador.EmailPessoal,
                    "Avaliação Concluída",
                    "O seu gestor finalizou a sua avaliação. Aceda para ver o resultado.",
                    "/minhas-avaliacoes");
            }
            else
            {
                avaliacao.Estado = EstadoAvaliacao.EmAndamento;
            }

            await _context.SaveChangesAsync();
            return MapToDto(avaliacao);
        }

        // ... Implementar restantes métodos ...
        public Task<decimal> CalcularMediaFinalAsync(int avaliacaoId) => throw new NotImplementedException();
        public Task<List<AvaliacaoDto>> GetMinhasAvaliacoesAsync(string emailColaborador) => throw new NotImplementedException();
        public Task<CicloAvaliacaoDto> CriarCicloAsync(CriarCicloRequest request) => throw new NotImplementedException();

        // Helper
        private static AvaliacaoDto MapToDto(Avaliacao a)
        {
            return new AvaliacaoDto
            {
                Id = a.Id,
                NomeColaborador = a.Colaborador?.NomeCompleto ?? "N/A",
                NomeCiclo = a.Ciclo?.Nome ?? "N/A",
                Estado = a.Estado,
                NotaFinal = a.MediaFinal,
                Itens = a.Itens?.Select(i => new AvaliacaoItemDto
                {
                    Id = i.Id,
                    CompetenciaId = i.CompetenciaId,
                    NotaGestor = i.NotaGestor
                    // Mapear restantes campos...
                }).ToList() ?? new List<AvaliacaoItemDto>()
            };
        }
    }
}
