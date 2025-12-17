export interface AvaliacaoItem {
  id: string;
  tituloCompetencia: string;
  competenciaId: string;

  notaGestor: number | null; // Pode ser null se ainda não avaliou
  comentario: string;
  // Notas
  notaAutoAvaliacao?: number; // ? significa opcional
  // Comentários
  justificativaColaborador?: string;
  justificativaGestor?: string; // Era 'comentario' antes
  // Apenas para uso no frontend (nome da competência, se vier no DTO)
  nomeCompetencia?: string;
  descricaoCompetencia?: string;
}
