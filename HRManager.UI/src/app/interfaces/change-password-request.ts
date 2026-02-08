export interface ChangePasswordRequest {
  email: string;
  passwordAtual: string;
  novaPassword: string;
  confirmarNovaPassword: string;
}
