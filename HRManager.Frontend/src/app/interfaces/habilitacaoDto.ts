export interface HabilitacaoDto {
  id: string;
  grau: string; // "Licenciatura", "Mestrado", etc.
  curso: string;
  instituicao: string;
  dataConclusao: string;
  caminhoDocumento?: string;
}
