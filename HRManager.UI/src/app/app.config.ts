import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from '../core/interceptors/auth.interceptor';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async'; // <--- Importante para animações
import { errorInterceptor } from '../core/interceptors/error.interceptor';
import { provideToastr } from 'ngx-toastr';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

export const appConfig: ApplicationConfig = {
  providers: [
    // provideZoneChangeDetection({ eventCoalescing: true }),
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),

    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])), // Permite fazer pedidos HTTP (GET, POST, etc.)
    provideAnimationsAsync(), // <--- Adicione isto para os gráficos animarem
    provideCharts(withDefaultRegisterables()), // <--- Adicione isto para usar gráficos
    provideToastr({
      timeOut: 3000,
      positionClass: 'toast-top-right',
      preventDuplicates: true,
    })
  ]
};
