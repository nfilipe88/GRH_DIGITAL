export interface SubmeterAvaliacaoRequest {
  respostas: { itemId: number; nota: number; comentario: string }[];
  comentarioFinal: string;
  finalizar: boolean;
}
