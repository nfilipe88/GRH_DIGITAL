// *** INTERFACE ATUALIZADA ***
// Corresponde ao nosso novo ColaboradorListDto
export interface Colaborador {
  id: number;
  nomeCompleto: string;
  emailPessoal: string;
  nif: string;
  nomeInstituicao: string; // <-- Mudança principal
  cargo: string | null;
  isAtivo: boolean;
}
