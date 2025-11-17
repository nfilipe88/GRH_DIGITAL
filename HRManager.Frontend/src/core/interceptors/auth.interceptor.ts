// Em: src/core/interceptors/auth.interceptor.ts
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { catchError, throwError } from 'rxjs'; // 1. Importar catchError e throwError
import { Router } from '@angular/router';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router); // 2. Injetar o Router
  const token = authService.getToken();

  if (!token) {
    return next(req);
  }

  const authReq = req.clone({
    headers: req.headers.set('Authorization', `Bearer ${token}`),
  });

  // 3. Adicionar o .pipe() para apanhar erros
  return next(authReq).pipe(
    catchError((error: any) => {

      // Verificamos se é um erro HTTP e se o status é 401
      if (error instanceof HttpErrorResponse && error.status === 401) {

        // O token é inválido ou expirou!
        console.error('Token expirado ou inválido. A fazer logout.');

        // Faz logout (limpa o token) e redireciona para o login
        authService.logout();
        router.navigate(['/login'], { queryParams: { reason: 'expired' } });
      }

      // Reenvia o erro para que o serviço (ex: colaborador.service)
      // ainda o possa processar (ex: mostrar "Falha ao carregar")
      return throwError(() => error);
    })
  );
};
