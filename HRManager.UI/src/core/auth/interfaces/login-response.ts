export interface LoginResponse {
  token: string;
  mustChangePassword: boolean;
  // role: 'GestorMaster' | 'TenantAdmin' | 'Colaborador';
  // displayName: string;
  nomeUser?: string;
  email?: string;
}
