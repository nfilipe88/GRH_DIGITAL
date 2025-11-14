// Em: src/core/guards/auth.guard.ts
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

export const authGuard: CanActivateFn = (route, state) => {

  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    // Utilizador está logado, pode aceder à rota
    return true;
  }

  // Utilizador NÃO está logado, redireciona para a página de login
  router.navigate(['/login']);
  return false;
};
