// src/app/services/permission.service.ts
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable, map } from 'rxjs';
import { Permission } from '../interfaces/permission';

@Injectable({
  providedIn: 'root',
})
export class PermissionService {
  private apiUrl = `${environment.apiUrl}/Permissions`;

  constructor(private http: HttpClient) {}

  // Busca todas as permissões disponíveis
  getAllPermissions(): Observable<Permission[]> {
    return this.http.get<Permission[]>(this.apiUrl);
  }

  // Busca permissões de um usuário específico
  getUserPermissions(userId: string): Observable<Permission[]> {
    return this.http.get<Permission[]>(`${this.apiUrl}/user/${userId}`);
  }

  // Verifica se tem uma permissão específica
  hasPermission(permissionCode: string): boolean {
    const userPermissions = this.getStoredPermissions();
    return userPermissions.includes(permissionCode);
  }

  // Busca permissões do usuário atual do localStorage
  getStoredPermissions(): string[] {
    try {
      const userData = localStorage.getItem('userData');
      if (userData) {
        const user = JSON.parse(userData);
        return user.permissions || [];
      }
    } catch (error) {
      console.error('Erro ao ler permissões do localStorage:', error);
    }
    return [];
  }

  // Carrega permissões do usuário atual da API
  loadCurrentUserPermissions(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/current-user`).pipe(
      map(permissions => {
        this.storePermissions(permissions);
        return permissions;
      })
    );
  }

  // Armazena permissões no localStorage
  storePermissions(permissions: string[]): void {
    try {
      const userData = localStorage.getItem('userData');
      if (userData) {
        const user = JSON.parse(userData);
        user.permissions = permissions;
        localStorage.setItem('userData', JSON.stringify(user));
      }
    } catch (error) {
      console.error('Erro ao armazenar permissões:', error);
    }
  }

  // Limpa permissões
  clearPermissions(): void {
    try {
      const userData = localStorage.getItem('userData');
      if (userData) {
        const user = JSON.parse(userData);
        delete user.permissions;
        localStorage.setItem('userData', JSON.stringify(user));
      }
    } catch (error) {
      console.error('Erro ao limpar permissões:', error);
    }
  }

  // Verifica se o usuário tem alguma das permissões fornecidas
  hasAnyPermission(permissionCodes: string[]): boolean {
    if (!permissionCodes || permissionCodes.length === 0) return true;

    const userPermissions = this.getStoredPermissions();
    return permissionCodes.some(code => userPermissions.includes(code));
  }

  // Verifica se o usuário tem todas as permissões fornecidas
  hasAllPermissions(permissionCodes: string[]): boolean {
    if (!permissionCodes || permissionCodes.length === 0) return true;

    const userPermissions = this.getStoredPermissions();
    return permissionCodes.every(code => userPermissions.includes(code));
  }
}
