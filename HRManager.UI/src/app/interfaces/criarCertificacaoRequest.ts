export interface CriarCertificacaoRequest {
  nomeCertificacao: string;
  entidadeEmissora: string;
  dataEmissao: string;
  dataValidade?: string;
  documento?: File;
}
