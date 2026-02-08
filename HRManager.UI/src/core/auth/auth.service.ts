import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { LoginResponse } from '../../core/auth/interfaces/login-response';
import { UserDetailsDto } from '../../app/interfaces/userDetailsDto';
import { PagedResult } from '../../app/interfaces/paged-result';
import { ChangePasswordRequest } from '../../app/interfaces/change-password-request';
import { RegisterRequest } from '../../app/interfaces/registerRequest';
import { UserListDto } from '../../app/interfaces/userListDto';
import { environment } from '../../environments/environment';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private apiUrl = `${environment.apiUrl}/Auth`;
  private readonly TOKEN_KEY = 'hr_manager_token'; // Chave para o localStorage

  // *** 1. ADICIONAR UMA CHAVE PARA O CARGO (ROLE) ***
  private readonly ROLE_KEY = 'hr_manager_role';
  private readonly USER_DATA_KEY = 'userData';

  constructor() { }

  /**
   * Tenta fazer login na API
   */
  public login(credentials: { email: string; password: string }): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap((response) => {
        localStorage.setItem(this.TOKEN_KEY, response.token);

        var nomeInstituicao = this.getInstituicaoNome() || '';

        // Decodifica o token para obter informações do usuário
        const decodedToken = this.getDecodedToken();
        if (decodedToken) {
          const userData = {
            id: decodedToken.nameid || decodedToken.sub,
            email: decodedToken.email,
            name: response.nomeUser || decodedToken.name,
            roles: this.getUserRoles(),
            instituicaoId: decodedToken.tenantId || decodedToken.InstituicaoId,
            instituicaoNome: decodedToken.InstituicaoNome || nomeInstituicao,
            isMaster: this.isMaster(),
            mustChangePassword: response.mustChangePassword || false
          };
          localStorage.setItem(this.USER_DATA_KEY, JSON.stringify(userData));
        }
      })
    );
  }

  // Método para buscar permissões após login
  public fetchUserPermissions(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/permissions`);
  }

  // --- METODO PARA ALTERAR SENHA NO PRIMEIRO LOGIN ---
  changePassword(data: ChangePasswordRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/change-password`, data);
  }
  // ----------------------------

  /**
   * Faz logout, limpando o token e redirecionando
   */
  public logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_DATA_KEY);
    this.router.navigate(['/login']);
  }

  public getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  public getUserData(): any {
    const userData = localStorage.getItem(this.USER_DATA_KEY);
    return userData ? JSON.parse(userData) : null;
  }

  private getDecodedToken(): any {
    const token = this.getToken();
    if (!token) return null;
    try {
      return jwtDecode(token);
    } catch {
      return null;
    }
  }

  // Obtém o perfil do utilizador (Role)
  public getUserRoles(): string[] {
    const token = this.getToken();
    if (!token) return [];

    const payload = this.getDecodedToken();
    const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
                      payload['role'] ||
                      payload['Role'];

    if (!roleClaim) return [];

    return Array.isArray(roleClaim) ? roleClaim : [roleClaim];
  }

  // Verifica se o utilizador tem permissão
  // E o hasRole verificaria se ALGUMA das roles do utilizador bate certo
  public hasRole(allowedRoles: string[]): boolean {
    const userRoles = this.getUserRoles();
    return allowedRoles.some(r => userRoles.includes(r));
  }

  public isLoggedIn(): boolean {
    return this.getToken() !== null;
  }

  public register(data: RegisterRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register`, data);
  }

  public getUserDetails(): Observable<UserDetailsDto> {
    return this.http.get<UserDetailsDto>(`${this.apiUrl}/me`);
  }

  // *** 2. MÉTODO para listar utilizadores ***
  getAllUsers(page: number = 1, pageSize: number = 10): Observable<PagedResult<UserListDto>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<UserListDto>>(`${this.apiUrl}/users`, { params });
  }

  public isMaster(): boolean {
    const userRoles = this.getUserRoles();
    return userRoles.includes('GestorMaster');
  }

  public getInstituicaoId(): string | null {
    if (this.isMaster()) {
      return null;
    }

    const payload = this.getDecodedToken();
    return payload?.tenantId || payload?.InstituicaoId || null;
  }

  public getInstituicaoNome(): string | null {
    const userData = this.getUserData();
    return userData?.instituicaoNome || null;
  }

  // public getInstituicaoNome(): string | null {
  //   const token = this.getToken();
  //   if (!token) return null;

  //   try {
  //     const payloadPart = token.split('.')[1];
  //     const base64 = payloadPart.replace(/-/g, '+').replace(/_/g, '/');
  //     const jsonPayload = decodeURIComponent(
  //       atob(base64)
  //         .split('')
  //         .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
  //         .join('')
  //     );

  //     const payload = JSON.parse(jsonPayload);
  //     return payload.InstituicaoNome || null; // Retorna o nome que adicionámos no C#
  //   } catch (error) {
  //     return null;
  //   }
  // }

  // 3. Helper para obter o objeto User completo (se ainda não tiveres)
  getUser(): UserDetailsDto | null {
    const userStr = localStorage.getItem('user'); // Ou a tua chave de storage
    return userStr ? JSON.parse(userStr) : null;
  }

  // Método para atualizar dados do usuário no localStorage
  public updateUserData(data: any): void {
    const currentData = this.getUserData() || {};
    const updatedData = { ...currentData, ...data };
    localStorage.setItem(this.USER_DATA_KEY, JSON.stringify(updatedData));
  }
}
