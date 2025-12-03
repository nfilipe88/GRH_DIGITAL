export interface ColaboradorListDto {
  id: number;
  nomeCompleto: string;
  emailPessoal: string;
  nif: string;
  cargo: string | null;
  departamento: string;
  nomeInstituicao: string;
  isAtivo: boolean;
}
