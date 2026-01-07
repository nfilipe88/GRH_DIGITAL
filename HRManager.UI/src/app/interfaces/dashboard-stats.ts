import { DepartamentoStat } from "./departamentos-stats";

// Esta é a definição correta e única,
// baseada no DTO da sua API (DashboardStatsDto.cs)
export interface DashboardStats {
  // Gestor
  totalColaboradores: number;
  totalAusenciasAtivas: number;
  avaliacoesEmAndamento: number;
  colaboradoresPorDepartamento: DepartamentoStat[];

  // Colaborador
  meusDiasFeriasDisponiveis: number;
  proximaAvaliacaoEstado: string;
  minhasDeclaracoesPendentes: number;

  // Controlo
  isVisaoGestor: boolean;

  totalInstituicoes: number;
  totalUtilizadores: number;
  totalColaboradoresAtivos: number;
  ausenciasPendentes: number;
  novasAdmissoesMes: number;
  colaboradoresAusentesHoje: string[];
  ausenciasHoje: number;
  pedidosPendentes: number;
  distribuicaoDepartamento: DepartamentoStat[];
}
