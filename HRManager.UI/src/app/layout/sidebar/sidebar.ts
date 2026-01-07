import { CommonModule } from '@angular/common';
import { Component, inject, Input } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterModule } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-sidebar',
  imports: [CommonModule, RouterLink, RouterModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  @Input() isSidebarOpen: boolean = true;
  private authService = inject(AuthService);
  private router = inject(Router);

  userRole: string | null = null;

  ngOnInit() {
    this.userRole = this.authService.getUserRole() || '';
  }

  // --- Helpers de Permissão ---

  // Apenas para o Dono do Sistema (Vê Instituições e Utilizadores de Sistema)
  get isMaster(): boolean {
    return this.userRole === 'GestorMaster';
  }

  // Para quem gere RH (Master ou Gestor de RH da empresa)
  // Vê Colaboradores, Configurações de Avaliação, Aprovações
  get isGestor(): boolean {
    return this.userRole === 'GestorMaster' || this.userRole === 'GestorRH';
  }

  // Todos veem (não precisa de getter, é o padrão), mas se quiseres isolar Colaboradores:
  get isColaborador(): boolean {
    return !!this.userRole; // Qualquer utilizador logado
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
