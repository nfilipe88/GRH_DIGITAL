// src/app/services/menu-permission.service.ts
import { Injectable } from '@angular/core';
import { PermissionService } from './permission.service';
import { AuthService } from '../../core/auth/auth.service';

@Injectable({
  providedIn: 'root'
})
export class MenuPermissionService {
  constructor(
    private permissionService: PermissionService,
    private authService: AuthService
  ) {}

  canShowMenuItem(item: MenuItem): boolean {
    // Se for GestorMaster, mostra tudo
    if (this.authService.isMaster()) {
      return true;
    }

    // Verifica roles se especificado
    if (item.requiredRoles && item.requiredRoles.length > 0) {
      const hasRole = this.authService.hasRole(item.requiredRoles);
      if (!hasRole) return false;
    }

    // Verifica permissões
    if (item.requiredPermissions && item.requiredPermissions.length > 0) {
      return this.permissionService.hasAnyPermission(item.requiredPermissions);
    }

    return true;
  }

  filterMenuItems(items: MenuItem[]): MenuItem[] {
    return items.filter(item => {
      if (item.children && item.children.length > 0) {
        item.children = this.filterMenuItems(item.children);
        return item.children.length > 0;
      }
      return this.canShowMenuItem(item);
    });
  }

  getMainMenu(): MenuItem[] {
    const menu: MenuItem[] = [
      {
        path: '/dashboard',
        label: 'Dashboard',
        icon: 'dashboard',
        requiredPermissions: []
      },
      {
        path: '/colaboradores',
        label: 'Colaboradores',
        icon: 'people',
        requiredPermissions: ['COLABORADORES_VIEW'],
        requiredRoles: ['GestorRH', 'GestorMaster']
      },
      {
        path: '/instituicoes',
        label: 'Instituições',
        icon: 'business',
        requiredPermissions: ['INSTITUTIONS_VIEW'],
        requiredRoles: ['GestorMaster']
      },
      {
        path: '/gestao-ausencias',
        label: 'Gestão de Ausências',
        icon: 'calendar_today',
        requiredPermissions: ['AUSENCIAS_VIEW'],
        requiredRoles: ['GestorRH', 'GestorMaster']
      },
      {
        path: '/utilizadores',
        label: 'Utilizadores',
        icon: 'person',
        requiredPermissions: ['USERS_VIEW'],
        requiredRoles: ['GestorRH', 'GestorMaster']
      },
      {
        path: '/gestao-roles',
        label: 'Gestão de Permissões',
        icon: 'security',
        requiredPermissions: ['ROLES_VIEW', 'PERMISSIONS_VIEW'],
        requiredRoles: ['GestorMaster']
      },
      {
        path: '/minhas-ausencias',
        label: 'Minhas Ausências',
        icon: 'event_available',
        requiredPermissions: ['AUSENCIAS_VIEW_SELF']
      },
      {
        path: '/avaliacoes',
        label: 'Avaliações',
        icon: 'assessment',
        requiredPermissions: ['AVALIACOES_VIEW_SELF', 'AVALIACOES_MANAGE_TEAM'],
        children: [
          {
            path: '/avaliacoes/minhas-avaliacoes',
            label: 'Minhas Avaliações',
            requiredPermissions: ['AVALIACOES_VIEW_SELF']
          },
          {
            path: '/avaliacoes/configuracao',
            label: 'Configuração',
            requiredPermissions: ['AVALIACOES_CONFIGURE'],
            requiredRoles: ['GestorRH', 'GestorMaster']
          },
          {
            path: '/avaliacoes/equipa',
            label: 'Avaliações da Equipa',
            requiredPermissions: ['AVALIACOES_MANAGE_TEAM'],
            requiredRoles: ['GestorRH', 'GestorMaster']
          }
        ]
      }
    ];

    return this.filterMenuItems(menu);
  }
}
