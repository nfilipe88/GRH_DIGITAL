export interface AvaliacaoItem {
  id: number;
  tituloCompetencia: string;
  descricaoCompetencia: string;
  notaGestor: number | null; // Pode ser null se ainda não avaliou
  comentario: string;
}
