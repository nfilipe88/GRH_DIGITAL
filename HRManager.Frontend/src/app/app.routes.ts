import { Routes } from '@angular/router';
import { GestaoInstituicoes } from './pages/gestao-instituicoes/gestao-instituicoes';
import { GestaoColaboradores } from './pages/gestao-colaboradores/gestao-colaboradores';
import { authGuard } from '../core/guards/auth.guard';
import { GestaoUtilizadores } from './pages/gestao-utilizadores/gestao-utilizadores';
import { Dashboard } from './pages/dashboard/dashboard';
import { loginGuard } from '../core/guards/login.guards';
import { AdminLayout } from './layout/admin-layout/admin-layout';
import { Login } from '../core/auth/login/login';

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
    ]
  },

  // Rota "catch-all" para redirecionar para o login ou dashboard
  { path: '**', redirectTo: 'login' } // Ou para 'dashboard', dependendo da sua lógica
];
