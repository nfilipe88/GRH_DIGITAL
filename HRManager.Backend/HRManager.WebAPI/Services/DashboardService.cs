using HRManager.Application.Interfaces;
using HRManager.WebAPI.Constants;
using HRManager.WebAPI.Domain.enums;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
using HRManager.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManager.WebAPI.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly HRManagerDbContext _context;
        private readonly ITenantService _tenantService;

        public DashboardService(HRManagerDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        //public async Task<DashboardStatsDto> GetStatsAsync()
        //{
        //    var hoje = DateTime.UtcNow.Date;
        //    var trintaDiasAtras = hoje.AddDays(-30);

        //    // 1. Total Colaboradores (Ativos)
        //    var totalColaboradores = await _context.Colaboradores
        //        .CountAsync(c => c.IsAtivo);

        //    // 2. Ausências HOJE (Aprovadas e dentro do prazo)
        //    var ausenciasHoje = await _context.Ausencias
        //        .CountAsync(a => a.Estado == EstadoAusencia.Aprovada
        //                         && a.DataInicio <= hoje
        //                         && a.DataFim >= hoje);

        //    // 3. Pedidos Pendentes (Para o RH atuar)
        //    var pendentes = await _context.Ausencias
        //        .CountAsync(a => a.Estado == EstadoAusencia.Pendente);

        //    // 4. Gráfico: Distribuição por Departamento
        //    var porDepartamento = await _context.Colaboradores
        //        .Where(c => c.IsAtivo && !string.IsNullOrEmpty(c.Departamento))
        //        .GroupBy(c => c.Departamento)
        //        .Select(g => new DepartamentoStatDto
        //        {
        //            Nome = g.Key,
        //            Quantidade = g.Count()
        //        })
        //        .ToListAsync();

        //    // 4. Lógica de Novas Admissões (Últimos 30 dias)
        //    var novasAdmissoes = await _context.Colaboradores
        //        .CountAsync(c => c.IsAtivo && c.DataAdmissao >= trintaDiasAtras);

        //    return new DashboardStatsDto
        //    {
        //        TotalColaboradores = totalColaboradores,
        //        AusenciasHoje = ausenciasHoje,
        //        PedidosPendentes = pendentes,
        //        NovasAdmissoesMes = novasAdmissoes,
        //        DistribuicaoDepartamento = porDepartamento
        //    };
        //}

        // Adicionamos o userId e roles como parâmetros
        public async Task<DashboardStatsDto> GetDashboardStatsAsync(string email, List<string> roles)
        {
            var stats = new DashboardStatsDto();

            // Identificar Roles
            bool isMaster = roles.Contains(RolesConstants.GestorMaster);
            bool isGestor = isMaster || roles.Contains(RolesConstants.GestorRH);

            stats.IsVisaoGestor = isGestor;
            var hojeUtc = DateTime.UtcNow.Date;

            if (isGestor)
            {
                // --- TRUQUE: Função Local para aplicar "God Mode" ---
                // Se for Master, ignora os filtros globais (vê todas as instituições).
                // Se não for, aplica os filtros normais do DbContext.
                IQueryable<T> Query<T>() where T : class
                {
                    var q = _context.Set<T>().AsQueryable();
                    if (isMaster) return q.IgnoreQueryFilters();
                    return q;
                }
                // ----------------------------------------------------

                // Agora usamos Query<T>() em vez de _context.T
                stats.TotalColaboradores = await Query<Colaborador>()
                    .CountAsync(c => c.IsAtivo);

                stats.TotalAusenciasAtivas = await Query<Ausencia>()
                    .CountAsync(a => a.DataInicio <= hojeUtc && a.DataFim >= hojeUtc && a.Estado == EstadoAusencia.Aprovada);

                stats.AvaliacoesEmAndamento = await Query<Avaliacao>()
                    .CountAsync(a => a.Estado == EstadoAvaliacao.EmAndamento || a.Estado == EstadoAvaliacao.EmAndamento);

                stats.ColaboradoresPorDepartamento = await Query<Colaborador>()
                    .Where(c => c.IsAtivo && !string.IsNullOrEmpty(c.Departamento))
                    .GroupBy(c => c.Departamento)
                    .Select(g => new DepartamentoStatDto { Nome = g.Key, Quantidade = g.Count() })
                    .ToListAsync();
            }
            else
            {
                // Lógica do Colaborador (Mantém-se igual, pois ele só deve ver o seu)
                var colaborador = await _context.Colaboradores
                    .FirstOrDefaultAsync(c => c.EmailPessoal == email);

                if (colaborador != null)
                {
                    var anoAtual = DateTime.UtcNow.Year;

                    var diasGastos = await _context.Ausencias
                        .Where(a => a.ColaboradorId == colaborador.Id
                                    && a.Estado == EstadoAusencia.Aprovada
                                    && a.Tipo == TipoAusencia.Ferias
                                    && a.DataInicio.Year == anoAtual)
                        .SumAsync(a => (a.DataFim - a.DataInicio).Days + 1);

                    stats.MeusDiasFeriasDisponiveis = (colaborador.SaldoFerias > 0 ? colaborador.SaldoFerias : 22) - diasGastos;

                    var avaliacaoAtual = await _context.Avaliacoes
                        .Include(a => a.Ciclo)
                        .Where(a => a.ColaboradorId == colaborador.Id && a.Ciclo.IsAtivo)
                        .OrderByDescending(a => a.DataCriacao)
                        .FirstOrDefaultAsync();

                    stats.ProximaAvaliacaoEstado = avaliacaoAtual?.Estado.ToString() ?? "Sem ciclo ativo";

                    stats.MinhasDeclaracoesPendentes = await _context.PedidosDeclaracao
                        .CountAsync(p => p.ColaboradorId == colaborador.Id && p.Estado == EstadoPedidoDeclaracao.Pendente);
                }
            }

            return stats;
        }
        //public async Task<DashboardStatsDto> GetStatsAsync(Guid? instituicaoId = null)
        //{
        //    // Implemente a lógica baseada no instituicaoId
        //    var hoje = DateTime.UtcNow.Date;
        //    var trintaDiasAtras = hoje.AddDays(-30);

        //    // 1. Total Colaboradores (Ativos) - com filtro por instituição se fornecido
        //    var queryColaboradores = _context.Colaboradores.Where(c => c.IsAtivo);

        //    if (instituicaoId.HasValue)
        //    {
        //        queryColaboradores = queryColaboradores.Where(c => c.InstituicaoId == instituicaoId.Value);
        //    }

        //    var totalColaboradores = await queryColaboradores.CountAsync();

        //    // 2. Ausências HOJE
        //    var queryAusenciasHoje = _context.Ausencias
        //        .Where(a => a.Estado == EstadoAusencia.Aprovada
        //                    && a.DataInicio <= hoje
        //                    && a.DataFim >= hoje);

        //    if (instituicaoId.HasValue)
        //    {
        //        queryAusenciasHoje = queryAusenciasHoje.Where(a => a.Colaborador.InstituicaoId == instituicaoId.Value);
        //    }

        //    var ausenciasHoje = await queryAusenciasHoje.CountAsync();

        //    // 3. Pedidos Pendentes
        //    var queryPendentes = _context.Ausencias
        //        .Where(a => a.Estado == EstadoAusencia.Pendente);

        //    if (instituicaoId.HasValue)
        //    {
        //        queryPendentes = queryPendentes.Where(a => a.Colaborador.InstituicaoId == instituicaoId.Value);
        //    }

        //    var pendentes = await queryPendentes.CountAsync();

        //    // 4. Distribuição por Departamento
        //    var queryDepartamentos = _context.Colaboradores
        //        .Where(c => c.IsAtivo && !string.IsNullOrEmpty(c.Departamento));

        //    if (instituicaoId.HasValue)
        //    {
        //        queryDepartamentos = queryDepartamentos.Where(c => c.InstituicaoId == instituicaoId.Value);
        //    }

        //    var porDepartamento = await queryDepartamentos
        //        .GroupBy(c => c.Departamento)
        //        .Select(g => new DepartamentoStatDto
        //        {
        //            Nome = g.Key,
        //            Quantidade = g.Count()
        //        })
        //        .ToListAsync();

        //    // 5. Novas Admissões (Últimos 30 dias)
        //    var queryNovasAdmissoes = _context.Colaboradores
        //        .Where(c => c.IsAtivo && c.DataAdmissao >= trintaDiasAtras);

        //    if (instituicaoId.HasValue)
        //    {
        //        queryNovasAdmissoes = queryNovasAdmissoes.Where(c => c.InstituicaoId == instituicaoId.Value);
        //    }

        //    var novasAdmissoes = await queryNovasAdmissoes.CountAsync();

        //    return new DashboardStatsDto
        //    {
        //        TotalColaboradores = totalColaboradores,
        //        AusenciasHoje = ausenciasHoje,
        //        PedidosPendentes = pendentes,
        //        NovasAdmissoesMes = novasAdmissoes,
        //        DistribuicaoDepartamento = porDepartamento,
        //        // Converte a lista para dicionário
        //        ColaboradoresPorDepartamento = porDepartamento.ToDictionary(d => d.Nome, d => d.Quantidade)
        //    };
        //}
    }
}
