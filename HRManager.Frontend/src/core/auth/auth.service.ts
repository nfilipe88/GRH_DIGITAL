import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { LoginResponse } from '../../core/auth/interfaces/login-response';

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

  /**
   * Tenta fazer login na API
   */
  public login(credentials: { email: string; password: string }): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap((response) => {
        // 1. Guardar o token no localStorage
        localStorage.setItem(this.TOKEN_KEY, response.token);
      })
    );
  }

  /**
   * Faz logout, limpando o token e redirecionando
   */
  public logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    this.router.navigate(['/login']); // Redireciona para a página de login
  }

  /**
   * Pega o token guardado
   */
  public getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
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

  // *** 2. NOVO MÉTODO ***
  public getUsers(): Observable<UserListDto[]> {
    return this.http.get<UserListDto[]>(`${this.apiUrl}/users`);
  }
}
