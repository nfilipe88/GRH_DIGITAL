export interface RegisterRequest {
  email: string;
  password: string;
  role: string;
  instituicaoId: string | null;
}
