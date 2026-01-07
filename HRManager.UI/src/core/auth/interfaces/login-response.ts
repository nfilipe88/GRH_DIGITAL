export interface LoginResponse {
  token: string;
  role: 'GestorMaster' | 'TenantAdmin' | 'Colaborador';
  displayName: string;
}
