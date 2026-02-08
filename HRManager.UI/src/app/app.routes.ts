// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { authGuard } from '../core/guards/auth.guard';
import { loginGuard } from '../core/guards/login.guards';
import { AdminLayout } from './layout/admin-layout/admin-layout';
import { Login } from '../core/auth/login/login';
import { PermissionGuard } from '../core/guards/permission.guard';

export const routes: Routes = [
  {
    path: 'login',
    component: Login,
    canActivate: [loginGuard]
  },
  {
    path: '',
    component: AdminLayout,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },

      // === DASHBOARD (Todos) ===
      {
        path: 'dashboard',
        loadComponent: () => import('./pages/dashboard/dashboard').then(m => m.Dashboard)
      },

      // === MÓDULO RH (Apenas Gestores) ===
      {
        path: 'colaboradores',
        loadComponent: () => import('./pages/gestao-colaboradores/gestao-colaboradores').then(m => m.GestaoColaboradores),
        canActivate: [PermissionGuard],
        data: { permissions: ['COLABORADORES_VIEW', 'COLABORADORES_MANAGE'] }
      },
      {
        path: 'instituicoes',
        loadComponent: () => import('./pages/gestao-instituicoes/gestao-instituicoes').then(m => m.GestaoInstituicoes),
        canActivate: [PermissionGuard],
        data: { permissions: ['INSTITUTIONS_VIEW', 'INSTITUTIONS_MANAGE'] }
      },
      {
        path: 'gestao-ausencias',
        loadComponent: () => import('./pages/gestao-ausencias/gestao-ausencias').then(m => m.GestaoAusencias),
        canActivate: [PermissionGuard],
        data: { permissions: ['AUSENCIAS_VIEW', 'AUSENCIAS_MANAGE'] }
      },
      {
        path: 'utilizadores',
        loadComponent: () => import('./pages/gestao-utilizadores/gestao-utilizadores').then(m => m.GestaoUtilizadores),
        canActivate: [PermissionGuard],
        data: { permissions: ['USERS_VIEW', 'USERS_MANAGE'] }
      },
      {
        path: 'gestao-roles',
        loadComponent: () => import('./pages/gestao-roles/gestao-roles').then(m => m.GestaoRoles),
        canActivate: [PermissionGuard],
        data: { permissions: ['ROLES_VIEW', 'ROLES_MANAGE'] }
      },
      {
        path: 'gestao-permissoes',
        loadComponent: () => import('./pages/gestao-permissoes/gestao-permissoes').then(m => m.GestaoPermissoesComponent),
        canActivate: [PermissionGuard],
        data: { permissions: ['PERMISSIONS_VIEW', 'PERMISSIONS_MANAGE'] }
      },

      // === MÓDULO PESSOAL (Colaborador + Gestores) ===
      {
        path: 'minhas-ausencias',
        loadComponent: () => import('./pages/minhas-ausencias/minhas-ausencias').then(m => m.MinhasAusencias),
        data: { permissions: ['AUSENCIAS_VIEW_SELF'] }
      },
      {
        path: 'gestao-calendario',
        loadComponent: () => import('./pages/gestao-calendario/gestao-calendario').then(m => m.GestaoCalendario),
        data: { permissions: ['CALENDAR_VIEW'] }
      },
      {
        path: 'emissao-declaracoes',
        loadComponent: () => import('./pages/emissao-declaracoes/emissao-declaracoes').then(m => m.EmissaoDeclaracoes),
        data: { permissions: ['DECLARACOES_EMIT'] }
      },
      {
        path: 'minhas-declaracoes',
        loadComponent: () => import('./pages/minhas-declaracoes/minhas-declaracoes').then(m => m.MinhasDeclaracoes),
        data: { permissions: ['DECLARACOES_VIEW_SELF'] }
      },
      {
        path: 'perfil',
        loadComponent: () => import('./pages/perfil/perfil').then(m => m.Perfil),
        data: { permissions: ['PROFILE_VIEW'] }
      },

      // === MÓDULO AVALIAÇÃO DE DESEMPENHO ===
      {
        path: 'minhas-avaliacoes',
        loadComponent: () => import('./pages/minhas-avaliacoes/minhas-avaliacoes').then(m => m.MinhasAvaliacoes),
        data: { permissions: ['AVALIACOES_VIEW_SELF'] }
      },
      {
        path: 'avaliacoes/configuracao',
        loadComponent: () => import('./pages/configuracao-avaliacao/configuracao-avaliacao').then(m => m.ConfiguracaoAvaliacao),
        canActivate: [PermissionGuard],
        data: { permissions: ['AVALIACOES_CONFIGURE'] }
      },
      {
        path: 'avaliacoes/equipa',
        loadComponent: () => import('./pages/lista-avaliacao/lista-avaliacao').then(m => m.ListaAvaliacao),
        canActivate: [PermissionGuard],
        data: { permissions: ['AVALIACOES_MANAGE_TEAM'] }
      },
      {
        path: 'avaliacoes/minha-equipa',
        redirectTo: 'avaliacoes/equipa'
      },
      {
        path: 'avaliacoes/realizar/:id',
        loadComponent: () => import('./pages/realizar-avaliacao/realizar-avaliacao').then(m => m.RealizarAvaliacao),
        canActivate: [PermissionGuard],
        data: { permissions: ['AVALIACOES_PERFORM'] }
      },
    ],
  },

  // === ROTAS PÚBLICAS ===
  {
    path: 'alterar-password',
    loadComponent: () => import('./pages/alterarpassword/alterarpassword').then(m => m.Alterarpassword)
  },

  // Catch-all
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
