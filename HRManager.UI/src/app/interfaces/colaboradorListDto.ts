export interface ColaboradorListDto {
  id: string;
  nomeCompleto: string;
  email: string;
  nif: string;
  cargo: string | null;
  nomeInstituicao?: string; // Torna opcional se não sempre estiver presente
  isAtivo: boolean;
}
