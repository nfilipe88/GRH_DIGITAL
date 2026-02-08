using HRManager.Application.Interfaces;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace HRManager.WebAPI.Services
{
    public class DeclaracaoService : IDeclaracaoService
    {
        private readonly HRManagerDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ITenantService _tenantService;

        public DeclaracaoService(
            HRManagerDbContext context,
            INotificationService notificationService,
            ITenantService tenantService)
        {
            _context = context;
            _notificationService = notificationService;
            _tenantService = tenantService;
        }

        public async Task<PedidoDeclaracaoDto> CriarPedidoAsync(CriarPedidoDeclaracaoRequest request, string emailColaborador)
        {
            var colaborador = await _context.Colaboradores
                .FirstOrDefaultAsync(c => c.EmailPessoal == emailColaborador);

            if (colaborador == null) throw new KeyNotFoundException("Colaborador não encontrado.");

            var pedido = new PedidoDeclaracao
            {
                ColaboradorId = colaborador.Id,
                Tipo = request.Tipo,
                Observacoes = request.Observacoes,
                DataSolicitacao = DateTime.UtcNow,
                Estado = EstadoPedidoDeclaracao.Pendente
            };

            _context.PedidosDeclaracao.Add(pedido);
            await _context.SaveChangesAsync();

            // Opcional: Notificar Gestores de RH que há um novo pedido
            // (Poderias implementar aqui uma lógica para buscar todos os users RH e notificar)

            return MapToDto(pedido);
        }

        public async Task<List<PedidoDeclaracaoDto>> GetMeusPedidosAsync(string emailColaborador)
        {
            var colaborador = await _context.Colaboradores.FirstOrDefaultAsync(c => c.EmailPessoal == emailColaborador);
            if (colaborador == null) return new List<PedidoDeclaracaoDto>();

            var lista = await _context.PedidosDeclaracao
                .Where(p => p.ColaboradorId == colaborador.Id)
                .OrderByDescending(p => p.DataSolicitacao)
                .ToListAsync();

            return lista.Select(MapToDto).ToList();
        }

        public async Task<List<PedidoDeclaracaoDto>> GetPedidosPendentesAsync()
        {
            // O Global Filter (Tenant) garante que o RH só vê da sua empresa
            var lista = await _context.PedidosDeclaracao
                .Include(p => p.Colaborador)
                .Where(p => p.Estado == EstadoPedidoDeclaracao.Pendente)
                .OrderBy(p => p.DataSolicitacao)
                .ToListAsync();

            return lista.Select(MapToDto).ToList();
        }

        public async Task<byte[]> GerarDeclaracaoPdfAsync(Guid idPedido, string emailGestor)
        {
            var pedido = await _context.PedidosDeclaracao
                .Include(p => p.Colaborador)
                .ThenInclude(c => c.Instituicao)
                .FirstOrDefaultAsync(p => p.Id == idPedido);

            if (pedido == null) throw new KeyNotFoundException("Pedido não encontrado.");
            // Verificação de segurança para evitar CS8602
            if (pedido.Colaborador?.Instituicao == null)
                throw new InvalidOperationException("Dados do colaborador ou instituição incompletos.");
            // 1. Gerar o Conteúdo (Simulação de PDF em texto para exemplo)
            // Em produção, usa QuestPDF ou iText7 aqui.
            var sb = new StringBuilder();
            sb.AppendLine($"DECLARAÇÃO: {pedido.Tipo}");
            sb.AppendLine($"A empresa {pedido.Colaborador.Instituicao.Nome} declara...");
            sb.AppendLine($"Que o colaborador {pedido.Colaborador.NomeCompleto}...");
            sb.AppendLine($"Data: {DateTime.Now:dd/MM/yyyy}");

            var pdfBytes = Encoding.UTF8.GetBytes(sb.ToString());

            // 2. Atualizar Estado
            pedido.Estado = EstadoPedidoDeclaracao.Concluido;
            pedido.DataConclusao = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // 3. --- NOTIFICAR O COLABORADOR (SINO NO FRONTEND) ---
            await _notificationService.NotifyUserByEmailAsync(
                pedido.Colaborador.EmailPessoal,
                "Declaração Emitida",
                $"A sua declaração de {pedido.Tipo} já está disponível para download.",
                "/minhas-declaracoes" // Link para onde ele deve clicar
            );

            return pdfBytes;
        }

        public async Task AtualizarEstadoPedidoAsync(Guid idPedido, bool aprovado, string emailGestor)
        {
            var pedido = await _context.PedidosDeclaracao
                .Include(p => p.Colaborador)
                .FirstOrDefaultAsync(p => p.Id == idPedido);

            if (pedido == null) throw new KeyNotFoundException("Pedido não encontrado.");

            pedido.Estado = aprovado ? EstadoPedidoDeclaracao.Pendente : EstadoPedidoDeclaracao.Rejeitado;
            await _context.SaveChangesAsync();

            // Notificar se for rejeitado
            if (!aprovado)
            {
                await _notificationService.NotifyUserByEmailAsync(
                    pedido.Colaborador.EmailPessoal,
                    "Pedido de Declaração Rejeitado",
                    $"O seu pedido de declaração foi rejeitado pelo RH.",
                    "/minhas-declaracoes"
                );
            }
        }

        private static PedidoDeclaracaoDto MapToDto(PedidoDeclaracao p)
        {
            return new PedidoDeclaracaoDto
            {
                Id = p.Id,
                Tipo = p.Tipo.ToString(),
                DataSolicitacao = p.DataSolicitacao,
                Estado = p.Estado.ToString(),
                NomeColaborador = p.Colaborador.NomeCompleto
            };
        }
    }
}
