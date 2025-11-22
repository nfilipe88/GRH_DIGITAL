
// Interface para o nosso DTO
export interface CriarColaboradorRequest {
  nomeCompleto: string;
  nif: string;
  numeroAgente?: number | null;
  emailPessoal: string;
  dataNascimento?: string | null; // Datas são enviadas como string
  dataAdmissao: string; //
  cargo?: string | null;
  tipoContrato?: string | null;
  salarioBase?: number | null;
  departamento?: string | null;
  localizacao?: string | null;
  instituicaoId?: string | null; // Guid é enviado como string
}
