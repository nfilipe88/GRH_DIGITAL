export interface CriarHabilitacaoRequest {
  grau: string;
  curso: string;
  instituicaoEnsino: string;
  dataConclusao: string;
  documento?: File;
}
