import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service'; // Ajusta o caminho se necessário

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // 1. Verificar se tem Token (Login básico)
  if (!authService.isLoggedIn()) {
    router.navigate(['/login']);
    return false;
  }

  // 2. Verificar Roles (se a rota exigir)
  const requiredRoles = route.data['roles'] as Array<string>;

  if (requiredRoles && requiredRoles.length > 0) {
    // Se a rota pede roles (ex: ['Admin']), verifica se o user tem
    if (!authService.hasRole(requiredRoles)) {
      // User logado, mas sem permissão -> Redireciona para Dashboard ou página de "Proibido"
      alert('Acesso negado: Não tens permissão para aceder a esta página.');
      router.navigate(['/dashboard']);
      return false;
    }
  }

  return true;
};
