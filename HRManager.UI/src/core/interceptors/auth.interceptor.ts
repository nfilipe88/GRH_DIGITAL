import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../auth/auth.service'; // <--- Importar o AuthService correto

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService); // <--- Injetar o AuthService
  const token = authService.getToken();    // <--- Usar o método do serviço (garante a chave correta)

  if (token) {
    // Clona o pedido e adiciona o cabeçalho Authorization
    const cloned = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(cloned);
  }

  return next(req);
};
