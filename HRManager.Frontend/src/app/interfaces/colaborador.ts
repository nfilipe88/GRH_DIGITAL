// *** INTERFACE ATUALIZADA ***
// Corresponde ao nosso novo ColaboradorListDto
export interface Colaborador {
  id: number;
  nomeCompleto: string;
  emailPessoal: string;
  nif: string;
  dataAdmissao: string;
  nomeInstituicao: string;
  tipoContrato: string;
  salarioBase: number;
  cargo: string | null;
  isAtivo: boolean;
  departamento: string;
  localizacao: string;
  morada: string;
  telemovel?: number | null;
  iban: string;
  saldoFerias: number;

}
