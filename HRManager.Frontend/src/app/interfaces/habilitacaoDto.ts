export interface HabilitacaoDto {
  id: number;
  grau: string; // "Licenciatura", "Mestrado", etc.
  curso: string;
  instituicao: string;
  dataConclusao: string;
  caminhoDocumento?: string;
}
