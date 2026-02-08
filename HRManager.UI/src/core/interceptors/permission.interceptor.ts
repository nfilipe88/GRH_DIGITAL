// src/app/core/interceptors/permission.interceptor.ts
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, switchMap, take } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { PermissionService } from '../../app/services/permission.service';

@Injectable()
export class PermissionInterceptor implements HttpInterceptor {
  private permissionsLoaded = false;

  constructor(
    private authService: AuthService,
    private permissionService: PermissionService,
    private router: Router
  ) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Se for uma requisição de login ou pública, não carrega permissões
    if (req.url.includes('/Auth/login') || req.url.includes('/Auth/register')) {
      return next.handle(req);
    }

    // Se o usuário está logado e as permissões ainda não foram carregadas
    if (this.authService.isLoggedIn() && !this.permissionsLoaded) {
      return this.permissionService.loadCurrentUserPermissions().pipe(
        take(1),
        switchMap(() => {
          this.permissionsLoaded = true;
          return next.handle(req);
        }),
        catchError(error => {
          console.error('Erro ao carregar permissões:', error);
          this.permissionsLoaded = true;
          return next.handle(req);
        })
      );
    }

    return next.handle(req);
  }
}
