import { Routes } from '@angular/router';
import { GestaoInstituicoes } from './pages/gestao-instituicoes/gestao-instituicoes';
import { GestaoColaboradores } from './pages/gestao-colaboradores/gestao-colaboradores';
import { authGuard } from '../core/guards/auth.guard';
import { Login } from './pages/login/login';
import { GestaoUtilizadores } from './pages/gestao-utilizadores/gestao-utilizadores';

export const routes: Routes = [
  // 3. Adicionar a rota de Login
  {
    path: 'login',
    component: Login,
  },

  // 4. Proteger as nossas rotas de gestão com o 'canActivate'
  {
    path: 'gestao-instituicoes',
    component: GestaoInstituicoes,
    canActivate: [authGuard] // <-- O "Segurança" está aqui
  },
  {
    path: 'gestao-colaboradores',
    component: GestaoColaboradores,
    canActivate: [authGuard] // <-- O "Segurança" está aqui
  },
  // *** 2. ADICIONE A NOVA ROTA PROTEGIDA ***
  {
    path: 'gestao-utilizadores',
    component: GestaoUtilizadores,
    canActivate: [authGuard]
  },

  // 5. Rota padrão: Se o utilizador aceder a 'localhost:4200'
  // redireciona para o login ou para a primeira página
  {
    path: '',
    redirectTo: 'gestao-instituicoes',
    pathMatch: 'full',
  },

  // (Opcional) Rota "catch-all" para redirecionar para o login
  {
    path: '**',
    redirectTo: 'login'
  }
];
