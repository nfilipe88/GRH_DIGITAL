using HRManager.Application.Interfaces;
using HRManager.WebAPI.Domain.enums;
using HRManager.WebAPI.Domain.Interfaces;
using HRManager.WebAPI.DTOs;
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

        public async Task<DashboardStatsDto> GetStatsAsync(Guid? instituicaoId = null)
        {
            // 1. Determinar o Tenant (Se for Master e passar ID, usa esse. Se não, usa o do Token)
            var tenantId = instituicaoId ?? _tenantService.GetTenantId();

            // Preparar query base de colaboradores (filtrada por tenant se aplicável)
            var queryColaboradores = _context.Colaboradores.AsQueryable();
            var queryAusencias = _context.Ausencias.AsQueryable();

            if (tenantId.HasValue)
            {
                queryColaboradores = queryColaboradores.Where(c => c.InstituicaoId == tenantId.Value);
                // Ausencias filtramos via navegação ou Join implícito, mas o Global Filter do EF já deve tratar disto.
                // Por segurança e clareza, aplicamos filtro se a entidade Ausencia tiver InstituicaoId ou via Colaborador.
                queryAusencias = queryAusencias.Where(a => a.Colaborador.InstituicaoId == tenantId.Value);
            }

            // 2. Executar Cálculos em Paralelo (ou sequencial rápido)

            var totalColaboradores = await queryColaboradores.CountAsync();

            var ativos = await queryColaboradores.CountAsync(c => c.IsAtivo);

            var ausenciasPendentes = await queryAusencias
                .CountAsync(a => a.Estado == EstadoAusencia.Pendente);

            // Admissões no mês atual
            var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).ToUniversalTime();
            var admissoesMes = await queryColaboradores
                .CountAsync(c => c.DataAdmissao >= inicioMes);

            // Agrupamento por Departamento (Top 5 + Outros se quiseres, aqui simplificado)
            var porDepartamento = await queryColaboradores
                .Where(c => !string.IsNullOrEmpty(c.Departamento))
                .GroupBy(c => c.Departamento)
                .Select(g => new { Dept = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.Dept, v => v.Count);

            // Quem está ausente hoje?
            var hoje = DateTime.UtcNow.Date;
            var ausentesHoje = await queryAusencias
                .Where(a => a.Estado == EstadoAusencia.Aprovada
                            && a.DataInicio <= hoje
                            && a.DataFim >= hoje)
                .Select(a => a.Colaborador.NomeCompleto)
                .Take(5) // Limitar a 5 para não encher o widget
                .ToListAsync();

            return new DashboardStatsDto
            {
                TotalColaboradores = totalColaboradores,
                TotalColaboradoresAtivos = ativos,
                AusenciasPendentes = ausenciasPendentes,
                NovasAdmissoesMes = admissoesMes,
                ColaboradoresPorDepartamento = porDepartamento,
                ColaboradoresAusentesHoje = ausentesHoje
            };
        }
    }
}
