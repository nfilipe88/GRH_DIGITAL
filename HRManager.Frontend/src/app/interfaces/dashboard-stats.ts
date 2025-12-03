// src/app/interfaces/dashboard-stats.interface.ts

// Esta é a definição correta e única,
// baseada no DTO da sua API (DashboardStatsDto.cs)
export interface DashboardStats {
  totalInstituicoes: number;
  totalColaboradores: number;
  totalUtilizadores: number;
  totalColaboradoresAtivos: number;
  ausenciasPendentes: number;
  novasAdmissoesMes: number;
  colaboradoresPorDepartamento: { [key: string]: number };
  colaboradoresAusentesHoje: string[];
}
