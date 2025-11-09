import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { LoginResponse } from './interfaces/login-response';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  // URL base, usa o proxy se estiver em desenvolvimento
  private apiUrl = '/api/auth/login';
  private readonly TOKEN_KEY = 'hr_auth_token';
  private readonly USER_ROLE_KEY = 'hr_user_role';

  constructor(private http: HttpClient) { }

  login(username: string, password: string): Observable<LoginResponse> {
    const payload = { username, password };

    return this.http.post<LoginResponse>(this.apiUrl, payload).pipe(
      tap(response => {
        // Armazenar o token e o papel no LocalStorage após sucesso
        localStorage.setItem(this.TOKEN_KEY, response.token);
        localStorage.setItem(this.USER_ROLE_KEY, response.role);

        // **Opcional:** Adicione aqui a lógica de navegação para o dashboard
      })
    );
  }

  // Utilizado pelo AuthInterceptor para adicionar o header
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  // Usado para proteção de rotas (Guards)
  getRole(): string | null {
    return localStorage.getItem(this.USER_ROLE_KEY);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_ROLE_KEY);
    // Redirecionar para a página de login
  }
}
