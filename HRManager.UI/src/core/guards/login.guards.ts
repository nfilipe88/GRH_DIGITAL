// Em: src/core/guards/login.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

/**
 * Protege a rota de login.
 * Se o utilizador já estiver logado, redireciona para o dashboard.
 */
export const loginGuard: CanActivateFn = () => {

  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    // Utilizador JÁ está logado, redireciona para o dashboard
    router.navigate(['/dashboard']);
    return false; // Bloqueia o acesso à página de login
  }

  // Utilizador NÃO está logado, pode aceder à página de login
  return true;
};
