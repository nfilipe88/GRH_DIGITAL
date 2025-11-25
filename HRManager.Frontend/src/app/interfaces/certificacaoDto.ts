export interface CertificacaoDto {
  id: number;
  nome: string;
  entidade: string;
  dataEmissao: string;
  dataValidade?: string;
  caminhoDocumento?: string;
}
