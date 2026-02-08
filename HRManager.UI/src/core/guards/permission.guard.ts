// src/app/core/guards/permission.guard.ts
import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { PermissionService } from '../../app/services/permission.service';

@Injectable({
  providedIn: 'root'
})
export class PermissionGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private permissionService: PermissionService,
    private router: Router
  ) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    // 1. Verificar se está autenticado
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return false;
    }

    // 2. Se for GestorMaster, permite acesso a tudo
    if (this.authService.isMaster()) {
      return true;
    }

    // 3. Verificar a permissão necessária da rota
    const requiredPermission = route.data['permission'] as string;
    const requiredPermissions = route.data['permissions'] as string[];

    // Se a rota não exige permissão específica, permite
    if (!requiredPermission && !requiredPermissions) {
      return true;
    }

    let hasPermission = false;

    // Verifica permissão única
    if (requiredPermission) {
      hasPermission = this.permissionService.hasPermission(requiredPermission);
    }

    // Verifica múltiplas permissões
    if (requiredPermissions && requiredPermissions.length > 0) {
      hasPermission = this.permissionService.hasAnyPermission(requiredPermissions);
    }

    if (!hasPermission) {
      // Redireciona para dashboard se não tiver permissão
      console.warn('Acesso negado: falta permissão', requiredPermission || requiredPermissions);
      this.router.navigate(['/dashboard']);
      return false;
    }

    return true;
  }
}
