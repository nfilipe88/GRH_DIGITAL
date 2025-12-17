import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { inject } from '@angular/core';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'Ocorreu um erro inesperado. Tente novamente mais tarde.';

      if (error.error instanceof ErrorEvent) {
        // Erro do lado do cliente (ex: rede em baixo)
        errorMessage = `Erro: ${error.error.message}`;
      } else {
        // Erro vindo do Backend (API)
        // Aqui tentamos ler a propriedade "Message" que definimos no Backend
        if (error.error && error.error.Message) {
          errorMessage = error.error.Message;
        } else if (typeof error.error === 'string') {
            errorMessage = error.error;
        }

        // Tratamento específico por código HTTP (Opcional)
        if (error.status === 401) {
           errorMessage = 'Sessão expirada ou inválida. Por favor faça login novamente.';
           // Aqui poderias redirecionar para o login automaticamente
        }
        if (error.status === 403) {
            errorMessage = 'Acesso negado. Não tem permissão para realizar esta ação.';
        }
      }

      // Exibe o erro (Podes trocar o alert por um Toast/Notificação mais bonito depois)
      alert(errorMessage);

      // Re-lança o erro para que o componente saiba que falhou, se necessário
      return throwError(() => error);
    })
  );
};
