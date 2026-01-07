import { Routes } from '@angular/router';
import { GestaoInstituicoes } from './pages/gestao-instituicoes/gestao-instituicoes';
import { GestaoColaboradores } from './pages/gestao-colaboradores/gestao-colaboradores';
import { authGuard } from '../core/guards/auth.guard';
import { GestaoUtilizadores } from './pages/gestao-utilizadores/gestao-utilizadores';
import { Dashboard } from './pages/dashboard/dashboard';
import { loginGuard } from '../core/guards/login.guards';
import { AdminLayout } from './layout/admin-layout/admin-layout';
import { Login } from '../core/auth/login/login';
import { MinhasAusencias } from './pages/minhas-ausencias/minhas-ausencias';
import { GestaoAusencias } from './pages/gestao-ausencias/gestao-ausencias';
import { GestaoCalendario } from './pages/gestao-calendario/gestao-calendario';
import { Perfil } from './pages/perfil/perfil';
import { EmissaoDeclaracoes } from './pages/emissao-declaracoes/emissao-declaracoes';
import { MinhasDeclaracoes } from './pages/minhas-declaracoes/minhas-declaracoes';
import { RealizarAvaliacao } from './pages/realizar-avaliacao/realizar-avaliacao';
import { ListaAvaliacao } from './pages/lista-avaliacao/lista-avaliacao';
import { ConfiguracaoAvaliacao } from './pages/configuracao-avaliacao/configuracao-avaliacao';

export const routes: Routes = [
    {
    path: 'login',
    component: Login,
    canActivate: [loginGuard] // Impede ir para login se já estiver logado
  },
  {
    path: '',
    component: AdminLayout,
    canActivate: [authGuard], // Protege todas as rotas filhas
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
        data: { role: ['GestorMaster', 'GestorRH'] }
      },
      {
        path: 'instituicoes',
        loadComponent: () => import('./pages/gestao-instituicoes/gestao-instituicoes').then(m => m.GestaoInstituicoes),
        data: { role: ['GestorMaster'] }
      },
      {
        path: 'gestao-ausencias',
        loadComponent: () => import('./pages/gestao-ausencias/gestao-ausencias').then(m => m.GestaoAusencias),
        data: { role: ['GestorMaster', 'GestorRH'] }
      },
      {
        path: 'utilizadores',
        loadComponent: () => import('./pages/gestao-utilizadores/gestao-utilizadores').then(m => m.GestaoUtilizadores),
        data: { role: ['GestorMaster', 'GestorRH'] }
      },

      // === MÓDULO PESSOAL (Colaborador + Gestores) ===
      {
        path: 'minhas-ausencias',
        loadComponent: () => import('./pages/minhas-ausencias/minhas-ausencias').then(m => m.MinhasAusencias)
      },
      {
        path: 'gestao-calendario',
        loadComponent: () => import('./pages/gestao-calendario/gestao-calendario').then(m => m.GestaoCalendario)
      },
      {
        path: 'emissao-declaracoes',
        loadComponent: () => import('./pages/emissao-declaracoes/emissao-declaracoes').then(m => m.EmissaoDeclaracoes)
      },
      {
        path: 'minhas-declaracoes',
        loadComponent: () => import('./pages/minhas-declaracoes/minhas-declaracoes').then(m => m.MinhasDeclaracoes)
      },
      {
        path: 'perfil',
        loadComponent: () => import('./pages/perfil/perfil').then(m => m.Perfil)
      },

      // === MÓDULO AVALIAÇÃO DE DESEMPENHO ===

      // 1. Minhas Avaliações (Autoavaliação e Consulta) - TODOS
      {
        path: 'minhas-avaliacoes',
        loadComponent: () => import('./pages/minhas-avaliacoes/minhas-avaliacoes').then(m => m.MinhasAvaliacoes)
      },

      // 2. Configuração (Só RH) - DUPLICADO REMOVIDO
      {
        path: 'avaliacoes/configuracao',
        loadComponent: () => import('./pages/configuracao-avaliacao/configuracao-avaliacao').then(m => m.ConfiguracaoAvaliacao),
        data: { role: ['GestorMaster', 'GestorRH'] }
      },

      // 3. Gerir/Iniciar Avaliações da Equipa (Só RH) - Rota da Lista
      {
        path: 'avaliacoes/equipa', // Mantive 'equipa' para ser consistente com o menu
        loadComponent: () => import('./pages/lista-avaliacao/lista-avaliacao').then(m => m.ListaAvaliacao),
        data: { role: ['GestorMaster', 'GestorRH'] }
      },

      // Rota antiga 'avaliacoes/minha-equipa' apontava para o mesmo componente.
      // Se não for usada no menu, pode ser removida. Se for usada, deixamos como alias:
      {
         path: 'avaliacoes/minha-equipa',
         redirectTo: 'avaliacoes/equipa'
      },

      // 4. O Gestor realiza a avaliação (Só RH) - DUPLICADO REMOVIDO
      {
        path: 'avaliacoes/realizar/:id',
        loadComponent: () => import('./pages/realizar-avaliacao/realizar-avaliacao').then(m => m.RealizarAvaliacao),
        data: { role: ['GestorMaster', 'GestorRH'] }
      },
    ],
  },

  // Catch-all
  { path: '**', redirectTo: 'login' }
];
