// Adicionei estas interfaces para melhorar a tipagem
export interface Instituicao {
  id: string;
  nome: string;
  nif: string;
  identificadorUnico: string;
  endereco: string;
  dataCriacao: string;
  telemovel? : number | null;
  emailContato: string;
  isAtiva: boolean;
}
