export interface CriarInstituicaoRequest {
  nome: string;
  identificadorUnico: string;
  nif: string;
  endereco: string;
  telemovel?: number | null;
  emailContato: string;
}
