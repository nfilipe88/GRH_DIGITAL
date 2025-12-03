import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { LoginResponse } from '../../core/auth/interfaces/login-response';
import { UserDetails } from '../../app/interfaces/userDetailsDto';

// *** 1. INTERFACE PARA O PEDIDO DE REGISTO ***
export interface RegisterRequest {
  email: string;
  password: string;
  role: string;
  instituicaoId: string | null;
}

// *** 1. NOVA INTERFACE ***
export interface UserListDto {
  id: number;
  email: string;
  role: string;
  nomeInstituicao: string | null;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private apiUrl = 'https://localhost:7234/api/Auth'; // A sua porta de backend
  private readonly TOKEN_KEY = 'hr_manager_token'; // Chave para o localStorage

  // *** 1. ADICIONAR UMA CHAVE PARA O CARGO (ROLE) ***
  private readonly ROLE_KEY = 'hr_manager_role';

  constructor() { }

  /**
   * Tenta fazer login na API
   */
  public login(credentials: { email: string; password: string }): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap((response) => {
        // *** 2. ATUALIZAR O LOGIN PARA GUARDAR AMBOS ***
        localStorage.setItem(this.TOKEN_KEY, response.token);
        localStorage.setItem(this.ROLE_KEY, response.role); // <-- Guardar o cargo
      })
    );
  }

  /**
   * Faz logout, limpando o token e redirecionando
   */
  public logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.ROLE_KEY); // <-- Limpar o cargo
    this.router.navigate(['/login']);
  }

  /**
   * Pega o token guardado
   */
  public getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  // *** 3. MÉTODO PARA LER O CARGO ***
  // public getUserRole(): string | null {
  //   return localStorage.getItem(this.ROLE_KEY);
  // }

  public getUserRole(): string | null {
    // Implementação similar para obter a role (ou ler do localStorage se já guardou lá)
    const token = this.getToken();
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      // O claim de role muitas vezes vem como "role" ou a URI longa da Microsoft
      return payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
    } catch {
      return null;
    }
  }

  /**
   * Verifica se o utilizador está logado (se existe um token)
   */
  public isLoggedIn(): boolean {
    return this.getToken() !== null;
  }

  /**
   * *** 2. MÉTODO DE REGISTO ***
   * Tenta registar um novo utilizador na API
   */
  public register(data: RegisterRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register`, data);
  }

  // *** /GETME MÉTODO ***
  /**
   * Obtém os detalhes do utilizador logado a partir da API
   */
  public getUserDetails(): Observable<UserDetails> {
    return this.http.get<UserDetails>(`${this.apiUrl}/me`);
  }

  // *** 2. NOVO MÉTODO ***
  public getUsers(): Observable<UserListDto[]> {
    return this.http.get<UserListDto[]>(`${this.apiUrl}/users`);
  }

  // --- NOVO MÉTODO: Extrair InstituicaoId do Token ---
  public getInstituicaoId(): string | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      // O JWT tem 3 partes separadas por pontos. A 2ª parte é o Payload (dados).
      const payloadPart = token.split('.')[1];

      // Descodificar Base64Url para JSON string
      const base64 = payloadPart.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );

      const payload = JSON.parse(jsonPayload);

      // A chave deve bater certo com "InstituicaoId" definido no TokenService.cs (C#)
      return payload.InstituicaoId || null;
    } catch (error) {
      console.error('Erro ao descodificar o token:', error);
      return null;
    }
  }

  public getInstituicaoNome(): string | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      const payloadPart = token.split('.')[1];
      const base64 = payloadPart.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );

      const payload = JSON.parse(jsonPayload);
      return payload.InstituicaoNome || null; // Retorna o nome que adicionámos no C#
    } catch (error) {
      return null;
    }
  }
}
