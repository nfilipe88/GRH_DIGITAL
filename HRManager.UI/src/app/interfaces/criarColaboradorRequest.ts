
// Interface para o nosso DTO
export interface CriarColaboradorRequest {
  nomeCompleto: string;
  nif: string;
  numeroAgente?: number | null;
  emailPessoal: string;
  morada: string;
  telemovel?: number | null;
  dataNascimento?: string | null; // Datas são enviadas como string
  dataAdmissao: string; //
  cargo?: string | null;
  tipoContrato?: string | null;
  salarioBase?: number | null;
  iban: string;
  departamento?: string | null;
  localizacao?: string | null;
  instituicaoId?: string | null; // Guid é enviado como string
}
