// src/core/guards/auth.guard.ts
import { inject } from '@angular/core';
import { Router } from '@angular/router';
// Ajuste o caminho para o seu AuthService.
// Vi que tem um em 'src/core/auth/auth.service.ts' e outro em 'src/app/services/auth.service.ts'
// Use o que for o principal (provavelmente o de 'core').
import { AuthService } from '../auth/auth.service';

export const authGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Assumindo que o seu AuthService tem um método para verificar o login
  // (ex: verificar se existe um token)
  if (authService.isLoggedIn()) {
    return true; // Permite o acesso
  }

  // Se não estiver logado, redireciona para a página de login
  return router.parseUrl('/login');
};
