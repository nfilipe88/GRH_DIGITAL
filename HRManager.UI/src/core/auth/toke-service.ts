import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class TokeService {
  private readonly TOKEN_KEY = 'hr_manager_token';

  saveToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }

  // Verifica se o utilizador tem token guardado
  hasToken(): boolean {
    return !!this.getToken();
  }
}
