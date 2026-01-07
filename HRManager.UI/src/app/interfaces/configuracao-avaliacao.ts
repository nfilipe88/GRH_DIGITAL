export interface Competencia {
  id: string;
  nome: string;
  descricao: string;
  tipo: number; // 0 = Comportamental, 1 = Técnica
  isAtiva: boolean;
}

export interface CriarCompetenciaRequest {
  nome: string;
  descricao: string;
  tipo: number;
}

export interface CicloAvaliacao {
  id: string;
  nome: string;
  dataInicio: string;
  dataFim: string;
  isAtivo: boolean;
}

export interface CriarCicloRequest {
  nome: string;
  dataInicio: string;
  dataFim: string;
}
