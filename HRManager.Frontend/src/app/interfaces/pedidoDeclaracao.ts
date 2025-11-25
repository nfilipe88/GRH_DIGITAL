export interface PedidoDeclaracaoDto {
  id: number;
  nomeColaborador: string;
  tipo: string; // "FinsBancarios", "VistoConsular", etc.
  estado: string; // "Pendente", "Concluido", "Rejeitado"
  dataSolicitacao: string;
  dataConclusao?: string;
  caminhoFicheiro?: string;
}
