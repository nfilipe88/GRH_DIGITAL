import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from '../core/interceptors/auth.interceptor';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async'; // <--- Importante para animações
// import { provideFormsModule } from '@angular/forms';

export const appConfig: ApplicationConfig = {
  providers: [
    // provideZoneChangeDetection({ eventCoalescing: true }),
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),

    // *** ADICIONE ISTO ***
    provideHttpClient(withInterceptors([authInterceptor])), // Permite fazer pedidos HTTP (GET, POST, etc.)
    provideAnimationsAsync() // <--- Adicione isto para os gráficos animarem
    // provideFormsModule()   // Permite usar ngModel e (ngSubmit) em formulários
  ]
};
