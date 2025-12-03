// *** INTERFACE: O modelo COMPLETO do Colaborador ***
// Isto é o que recebemos do endpoint GET /api/Colaboradores/{id}
export interface ColaboradorDetails {
  id: number;
  nomeCompleto: string;
  nif: string;
  numeroAgente: number | null;
  emailPessoal: string;
  dataNascimento: string | null; // Vem como string ISO
  dataAdmissao: string; // Vem como string ISO
  cargo: string | null;
  iban: string;
  telemovel: number | null;
  morada: string;
  tipoContrato: string | null;
  salarioBase: number | null;
  departamento: string | null;
  localizacao: string | null;
  instituicaoId: string;
}
