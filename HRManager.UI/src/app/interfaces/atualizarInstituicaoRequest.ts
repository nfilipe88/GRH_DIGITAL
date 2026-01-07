export interface AtualizarInstituicaoRequest {
  nome: string;
  identificadorUnico: string;
  nif: string;
  endereco: string;
  telemovel: number | null;
  emailContato: string;
}
