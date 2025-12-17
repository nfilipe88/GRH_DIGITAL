export interface CertificacaoDto {
  id: string;
  nome: string;
  entidade: string;
  dataEmissao: string;
  dataValidade?: string;
  caminhoDocumento?: string;
}
