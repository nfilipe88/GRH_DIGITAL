// O que recebemos na lista
export interface AusenciaDto {
  id: string;
  nomeColaborador: string;
  tipo: string; // "Ferias", "Doenca", etc.
  dataInicio: string;
  dataFim: string;
  diasTotal: number;
  estado: string; // "Pendente", "Aprovada", "Rejeitada"
  dataSolicitacao: string;
  caminhoDocumento?: string; // URL ou caminho para o documento carregado, se existir
}
