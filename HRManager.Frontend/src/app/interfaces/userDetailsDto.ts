// *** INTERFACE PARA CARREGAR DETALHES DO UTILIZADOR ***
export interface UserDetailsDto {
  id: string;
  email: string;
  roles: string[];
  nomeInstituicao?: string;
  instituicaoId?: string;
}
