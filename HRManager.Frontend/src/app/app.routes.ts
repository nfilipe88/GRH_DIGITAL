import { Routes } from '@angular/router';
import { GestaoInstituicoes } from './pages/gestao-instituicoes/gestao-instituicoes';

export const routes: Routes = [
  // 2. Adicione a rota
    { path: 'gestao-instituicoes', component: GestaoInstituicoes },

    // 3. (Opcional) Redirecionar a raiz para esta página por agora
    { path: '', redirectTo: 'gestao-instituicoes', pathMatch: 'full' }
];
