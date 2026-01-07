export interface PedidoDeclaracaoDto {
  id: string;
  colaboradorId: string;
  nomeColaborador: string;
  tipo: string; // "FinsBancarios", "VistoConsular", etc.
  estado: string; // "Pendente", "Concluido", "Rejeitado"
  dataSolicitacao: Date;
  dataConclusao?: Date;
  caminhoFicheiro?: string;
}
