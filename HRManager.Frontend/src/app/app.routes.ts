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

export const routes: Routes = [
  // 2. Rota pública de Login.
  // Será renderizada diretamente no <router-outlet> principal (de app.html)
  // Não tem layout.
  {
    path: 'login',
    component: Login,
    canActivate: [loginGuard] // Protege para não aceder ao login se já estiver logado
  },

  // 3. Rota "pai" que carrega o layout
  // Esta rota é protegida pelo authGuard.
  {
    path: '', // O "path" vazio agora aponta para o Layout
    component: AdminLayout,
    canActivate: [authGuard], // Se não estiver logado, o guard redireciona para /login

    // 4. Rotas "filhas" (children)
    // Estas rotas serão renderizadas DENTRO do <router-outlet> do AdminLayoutComponent
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: Dashboard },
      { path: 'instituicoes', component: GestaoInstituicoes },
      { path: 'colaboradores', component: GestaoColaboradores },
      { path: 'utilizadores', component: GestaoUtilizadores },
      { path: 'minhas-ausencias', component: MinhasAusencias },
      { path: 'gestao-ausencias', component: GestaoAusencias },
      { path: 'gestao-calendario', component: GestaoCalendario },
      { path: 'perfil', component: Perfil },
      { path: 'minhas-declaracoes', component: MinhasDeclaracoes },
      { path: 'emissao-declaracoes', component: EmissaoDeclaracoes },
      { path: 'avaliacoes/realizar/:id', component: RealizarAvaliacao },
      { path: 'avaliacoes/minha-equipa', component: ListaAvaliacao },
      {
        path: 'minhas-avaliacoes',
        loadComponent: () => import('./pages/minhas-avaliacoes/minhas-avaliacoes').then(m => m.MinhasAvaliacoes),
        canActivate: [authGuard] // Acessível a todos os perfis logados
      },
    ],
  },

  // Rota "catch-all" para redirecionar para o login ou dashboard
  { path: '**', redirectTo: 'login' } // Ou para 'dashboard', dependendo da sua lógica
];
