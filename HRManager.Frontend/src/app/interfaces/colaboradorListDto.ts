export interface ColaboradorListDto {
  id: string;
  nomeCompleto: string;
  emailPessoal: string;
  nif: string;
  cargo: string | null;
  departamento: string;
  nomeInstituicao: string;
  isAtivo: boolean;
}
