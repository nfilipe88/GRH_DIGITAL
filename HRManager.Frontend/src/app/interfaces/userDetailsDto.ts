// *** INTERFACE PARA CARREGAR DETALHES DO UTILIZADOR ***
export interface UserDetails {
  id: number;
  email: string;
  role: string;
  nomeInstituicao?: string;
}
