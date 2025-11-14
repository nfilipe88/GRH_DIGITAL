import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from '../core/interceptors/auth.interceptor';
// import { provideFormsModule } from '@angular/forms';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),

    // *** ADICIONE ISTO ***
    provideHttpClient(withInterceptors([authInterceptor])), // Permite fazer pedidos HTTP (GET, POST, etc.)
    // provideFormsModule()   // Permite usar ngModel e (ngSubmit) em formulários
  ]
};
